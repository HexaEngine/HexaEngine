namespace HexaEngine.Projects
{
    using LibGit2Sharp;
    using System;

    public static class ProjectVersionControl
    {
        private static readonly object _lock = new();
        private static readonly Identity identity;

        private static FileSystemWatcher? watcher;
        private static readonly List<string> changedFiles = new();
        private static bool filesChanged = true;
        private static Task? changedFilesTask;

        public static Repository? Repository { get; private set; }

        public static object SyncObject => _lock;

        static ProjectVersionControl()
        {
        }

        public static void Init()
        {
            if (ProjectManager.CurrentProjectFolder == null || Repository != null)
            {
                return;
            }

            var path = Repository.Discover(ProjectManager.CurrentProjectFolder);

            using HttpClient client = new();
            var gitIgnore = client.GetStringAsync("https://raw.githubusercontent.com/JunaMeinhold/HexaEngine/main/Templates/HexaEngine.gitignore").Result;
            File.WriteAllText(Path.Combine(ProjectManager.CurrentProjectFolder, ".gitignore"), gitIgnore);

            path ??= Repository.Init(ProjectManager.CurrentProjectFolder);

            Repository = new(path);

            CommitChanges("Initial Commit", false);

            InitInternal(ProjectManager.CurrentProjectFolder);
        }

        public static void TryInit()
        {
            if (ProjectManager.CurrentProjectFolder == null || Repository != null)
            {
                return;
            }

            var path = Repository.Discover(ProjectManager.CurrentProjectFolder);

            if (path == null)
            {
                return;
            }

            Repository = new(path);
            InitInternal(ProjectManager.CurrentProjectFolder);
        }

        private static void InitInternal(string path)
        {
            watcher = new(path);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.Changed += WatcherChanged;
            watcher.EnableRaisingEvents = true;
        }

        private static void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            filesChanged = true;
        }

        public static void CommitChanges(string message, bool amend)
        {
            if (Repository == null)
            {
                return;
            }

            // Stage all changes
            Commands.Stage(Repository, "*");

            // Create the commit author and committer
            var author = new Signature(identity, DateTimeOffset.Now);
            var committer = author;

            CommitOptions options = new();

            options.AmendPreviousCommit = amend;

            // Commit the changes
            Repository.Commit(message, author, committer, options);
        }

        public static void Fetch()
        {
            if (Repository == null)
            {
                return;
            }

            var remoteName = "origin";
            var remoteUrl = Repository.Network.Remotes[remoteName].Url;
            List<string> refspecs = ["+refs/heads/*:refs/remotes/origin/*"];
            FetchOptions options = new();

            options.CredentialsProvider = Method;

            Repository.Network.Fetch(remoteName, refspecs, options);
        }

        private static Credentials Method(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            return new DefaultCredentials();
        }

        public static void Unload()
        {
            Repository?.Dispose();
            Repository = null;
            watcher?.Dispose();
        }

        public static IEnumerable<string> GetChangedFiles()
        {
            if (Repository == null)
            {
                return Enumerable.Empty<string>();
            }

            if (filesChanged && (changedFilesTask == null || changedFilesTask.IsCompleted))
            {
                changedFilesTask = Task.Factory.StartNew(() =>
                {
                    // Get the current commit (HEAD)
                    var head = Repository.Head.Tip;

                    // Get the tree of the current commit
                    var commitTree = head.Tree;

                    // Compare the tree with the parent (previous) commit's tree
                    TreeChanges changes = Repository.Diff.Compare<TreeChanges>(commitTree, DiffTargets.WorkingDirectory);

                    // Retrieve the paths of changed files
                    lock (_lock)
                    {
                        changedFiles.Clear();
                        changedFiles.AddRange(changes.Select(change => change.Path));
                    }
                });

                filesChanged = false;
            }

            return changedFiles;
        }
    }
}