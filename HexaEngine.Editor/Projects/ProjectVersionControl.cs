namespace HexaEngine.Editor.Projects
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using HexaEngine.Security;
    using HexaEngine.Security.Credentials;
    using LibGit2Sharp;
    using System;

    public enum PasswordDialogMode
    {
        Password,
        UsernamePassword,
        UsernameToken,
    }

    public enum PasswordDialogResult
    {
        Cancel,
        Ok
    }

    public class PasswordDialog
    {
        private bool open;

        public PasswordDialog(PasswordDialogMode mode, Action<PasswordDialog, PasswordDialogResult> callback)
        {
            Mode = mode;
            Callback = callback;
        }

        public PasswordDialog(PasswordDialogMode mode)
        {
            Mode = mode;
            Callback = null;
        }

        public PasswordDialogMode Mode { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public PasswordDialogResult Result { get; private set; }

        public Action<PasswordDialog, PasswordDialogResult>? Callback { get; set; }

        public void Show()
        {
            open = true;
            while (open)
            {
                if (ImGui.Begin("Enter Password", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.Modal))
                {
                    switch (Mode)
                    {
                        case PasswordDialogMode.UsernamePassword:
                        case PasswordDialogMode.UsernameToken:
                            var username = Username;
                            ImGui.InputText("Username", ref username, 1);
                            Username = username;
                            break;
                    }
                    var password = Password;
                    ImGui.InputText(Mode == PasswordDialogMode.UsernameToken ? "Token" : "Password", ref password, 1, ImGuiInputTextFlags.Password);
                    Password = password;

                    if (ImGui.Button("Cancel"))
                    {
                        open = false;
                        Result = PasswordDialogResult.Cancel;
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Ok"))
                    {
                        open = false;
                        Result = PasswordDialogResult.Ok;
                    }
                }

                ImGui.End();
            }

            Callback?.Invoke(this, Result);
        }

        public Task ShowAsync()
        {
            open = true;
            while (open)
            {
                if (ImGui.Begin("Enter Password", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.Modal))
                {
                    switch (Mode)
                    {
                        case PasswordDialogMode.UsernamePassword:
                        case PasswordDialogMode.UsernameToken:
                            var username = Username;
                            ImGui.InputText("Username", ref username, 1);
                            Username = username;
                            break;
                    }
                    var password = Password;
                    ImGui.InputText(Mode == PasswordDialogMode.UsernameToken ? "Token" : "Password", ref password, 1, ImGuiInputTextFlags.Password);
                    Password = password;

                    if (ImGui.Button("Cancel"))
                    {
                        open = false;
                        Result = PasswordDialogResult.Cancel;
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Ok"))
                    {
                        open = false;
                        Result = PasswordDialogResult.Ok;
                    }
                }

                ImGui.End();
            }

            Callback?.Invoke(this, Result);

            return Task.CompletedTask;
        }
    }

    public static class ProjectVersionControl
    {
        private static readonly object _lock = new();
        private static readonly Identity identity;

        private static FileSystemWatcher? watcher;
        private static readonly List<string> changedFiles = [];
        private static bool filesChanged = true;
        private static Task? changedFilesTask;

        public static Repository? Repository { get; private set; }

        public static string HeadName => Repository.Head.FriendlyName;

        public static Branch Head => Repository.Head;

        public static object SyncObject => _lock;

        public static event Action<string>? FileChanged;

        static ProjectVersionControl()
        {
            string gitConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitconfig");
            if (File.Exists(gitConfigPath))
            {
                string[] lines = File.ReadAllLines(gitConfigPath);
                bool capture = false;
                string? name = null;
                string? email = null;

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        capture = false;
                        continue;
                    }

                    if (line.Contains("[user]"))
                    {
                        capture = true;
                    }

                    if (capture && line.Contains("name"))
                    {
                        name = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1];
                    }

                    if (capture && line.Contains("email"))
                    {
                        email = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1];
                    }
                }

                identity = new(name, email);
            }
            else
            {
                MessageBox.Show("Warning", $"Warning .gitconfig was not found, please make sure that {gitConfigPath} is present");
            }
        }

        public static GitCommitDifference CommitDifference { get; private set; } = new(0, 0);

        private static SecureUsernamePasswordCredentials CredentialsProvider(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            if (!CredentialsManager.IsOpen)
            {
                //PasswordDialog dialog = new(PasswordDialogMode.Password);
                // dialog.Show();
            }

            var creds = CredentialsManager.Container?.Get("git") as Security.Credentials.UsernamePasswordCredentials;
            return new SecureUsernamePasswordCredentials()
            {
                Username = creds?.Username.ConvertToString() ?? string.Empty,
                Password = creds?.Password ?? new(),
            };
        }

        public static void Init()
        {
            if (ProjectManager.CurrentProjectFolder == null || Repository != null)
            {
                return;
            }

            try
            {
                var path = Repository.Discover(ProjectManager.CurrentProjectFolder);

                using HttpClient client = new();
                var gitIgnore = client.GetStringAsync("https://raw.githubusercontent.com/JunaMeinhold/HexaEngine/main/Templates/HexaEngine.gitignore").Result;
                File.WriteAllText(Path.Combine(ProjectManager.CurrentProjectFolder, ".gitignore"), gitIgnore);

                path ??= Repository.Init(ProjectManager.CurrentProjectFolder);

                Repository = new(path);

                CommitChanges("Initial Commit", false);

                InitInternal(ProjectManager.CurrentProjectFolder);

                UpdateCommitDifference();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to init repo!");
                Logger.Log(ex);
                MessageBox.Show("Failed to init repo!", ex.Message);
            }
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

            try
            {
                Repository = new(path);
                InitInternal(ProjectManager.CurrentProjectFolder);
                FetchAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to init repo!");
                Logger.Log(ex);
                MessageBox.Show("Failed to init repo!", ex.Message);
            }
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

        public static Commit? CommitChanges(string message, bool amend)
        {
            if (Repository == null)
            {
                return null;
            }

            try
            {
                // Stage all changes
                Commands.Stage(Repository, "*");

                // Create the commit author and committer
                var author = new Signature(identity, DateTimeOffset.Now);
                var committer = author;

                CommitOptions options = new();

                options.AmendPreviousCommit = amend;

                // Commit the changes
                return Repository.Commit(message, author, committer, options);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to commit changes!");
                Logger.Log(ex);
                MessageBox.Show("Failed to commit changes!", ex.Message);
            }

            return null;
        }

        public static Task<Commit?> CommitChangesAsync(string message, bool amend)
        {
            return Task.Factory.StartNew(() => CommitChanges(message, amend));
        }

        public static Commit? CommitStaged(string message, bool amend)
        {
            if (Repository == null)
            {
                return null;
            }

            if (Repository.Index.Count == 0)
            {
                return null;
            }

            try
            {
                // Create the commit author and committer
                var author = new Signature(identity, DateTimeOffset.Now);
                var committer = author;

                CommitOptions options = new();

                options.AmendPreviousCommit = amend;

                // Commit the changes
                return Repository.Commit(message, author, committer, options);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to commit staged!");
                Logger.Log(ex);
                MessageBox.Show("Failed to commit staged!", ex.Message);
            }

            return null;
        }

        public static Task<Commit?> CommitStagedAsync(string message, bool amend)
        {
            return Task.Factory.StartNew(() => CommitStaged(message, amend));
        }

        public static void Fetch()
        {
            if (Repository == null)
            {
                return;
            }

            var remoteName = "origin";

            List<string> refspecs = ["+refs/heads/*:refs/remotes/origin/*"];

            FetchOptions options = new();
            options.CredentialsProvider = CredentialsProvider;

            try
            {
                Repository.Network.Fetch(remoteName, refspecs, options);
                UpdateCommitDifference();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed fetching origin!");
                Logger.Log(ex);
                MessageBox.Show("Failed fetching origin!", ex.Message);
            }
        }

        public static Task FetchAsync()
        {
            return Task.Factory.StartNew(Fetch);
        }

        public static void Push()
        {
            if (Repository == null)
            {
                return;
            }

            Push(Repository.Head);
        }

        public static void Push(Branch branch)
        {
            if (Repository == null)
            {
                return;
            }

            PushOptions options = new();
            options.CredentialsProvider = CredentialsProvider;

            try
            {
                Repository.Network.Push(branch, options);
                UpdateCommitDifference();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed push to origin!");
                Logger.Log(ex);
                MessageBox.Show("Failed push to origin!", ex.Message);
            }
        }

        public static Task PushAsync()
        {
            return Task.Factory.StartNew(Push);
        }

        public static MergeResult? Pull()
        {
            if (Repository == null)
            {
                return null;
            }

            PullOptions options = new();
            options.FetchOptions.CredentialsProvider = CredentialsProvider;

            var mergeOptions = new MergeOptions();
            var signature = Repository.Config.BuildSignature(DateTimeOffset.Now);

            try
            {
                MergeResult result = Commands.Pull(Repository, signature, options);
                UpdateCommitDifference();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed pull from origin!");
                Logger.Log(ex);
                MessageBox.Show("Failed pull from origin!", ex.Message);
            }

            return null;
        }

        public static Task<MergeResult?> PullAsync()
        {
            return Task.Factory.StartNew(Pull);
        }

        public static void Sync()
        {
            if (Repository == null)
            {
                return;
            }

            Pull();
            Push();
        }

        public static Task SyncAsync()
        {
            return Task.Factory.StartNew(Sync);
        }

        public static Branch? CreateBranch(string name)
        {
            if (Repository == null)
            {
                return null;
            }

            try
            {
                return Repository.CreateBranch(name);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create branch!");
                Logger.Log(ex);
                MessageBox.Show("Failed to create branch!", ex.Message);
            }

            return null;
        }

        public static void RenameBranch(Branch branch, string newName)
        {
            if (Repository == null)
            {
                return;
            }

            if (branch.IsRemote)
            {
                string oldName = Path.GetRelativePath(branch.RemoteName, branch.FriendlyName);
                Remote remote = Repository.Network.Remotes[branch.RemoteName];
                Repository.Network.Remotes.Update(branch.RemoteName, r => r.PushRefSpecs.Add($"+refs/heads/{oldName}:refs/heads/{newName}"));
                var oldUpstreamName = $"refs/heads/{oldName}";
                var newUpstreamName = $"refs/heads/{newName}";
                foreach (var otherBranch in Repository.Branches)
                {
                    if (otherBranch.IsRemote || otherBranch.UpstreamBranchCanonicalName != oldUpstreamName)
                    {
                        continue;
                    }

                    Repository.Branches.Update(otherBranch, b => b.UpstreamBranch = newUpstreamName);
                }
            }

            try
            {
                Repository.Branches.Rename(branch, newName);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to rename branch!");
                Logger.Log(ex);
                MessageBox.Show("Failed to rename branch!", ex.Message);
            }
        }

        public static void DeleteBranch(string name, bool isRemote)
        {
            if (Repository == null)
            {
                return;
            }

            try
            {
                Repository.Branches.Remove(name, isRemote);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to delete branch!");
                Logger.Log(ex);
                MessageBox.Show("Failed to delete branch!", ex.Message);
            }
        }

        public static void DeleteBranch(Branch branch)
        {
            DeleteBranch(branch.FriendlyName, branch.IsRemote);
        }

        public static Branch? CheckoutBranch(Branch branch)
        {
            if (Repository == null)
            {
                return null;
            }

            CheckoutOptions options = new();

            if (branch.IsRemote)
            {
                FetchOptions fetchOptions = new();
                fetchOptions.CredentialsProvider = CredentialsProvider;

                string localBranchName = Path.GetRelativePath(branch.RemoteName, branch.FriendlyName);

                List<string> refspecs = [$"+refs/heads/{localBranchName}:refs/remotes/{branch.RemoteName}/{localBranchName}"];

                try
                {
                    Repository.Network.Fetch(branch.RemoteName, refspecs, fetchOptions);

                    branch = Repository.CreateBranch(localBranchName, branch.Tip);
                    Repository.Branches.Update(branch, b => b.UpstreamBranch = $"refs/heads/{localBranchName}");
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to checkout remote branch!");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to checkout remote branch!", ex.Message);
                }
            }

            try
            {
                var result = Commands.Checkout(Repository, branch, options);
                UpdateCommitDifference();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to checkout branch!");
                Logger.Log(ex);
                MessageBox.Show("Failed to checkout branch!", ex.Message);
            }
            return null;
        }

        public static Task<Branch?> CheckoutBranchAsync(Branch branch)
        {
            return Task.Factory.StartNew(() => CheckoutBranch(branch));
        }

        public static Branch? CheckoutCommit(Commit commit)
        {
            if (Repository == null)
            {
                return null;
            }

            CheckoutOptions options = new();

            try
            {
                var result = Commands.Checkout(Repository, commit, options);
                UpdateCommitDifference();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to checkout commit!");
                Logger.Log(ex);
                MessageBox.Show("Failed to checkout commit!", ex.Message);
            }

            return null;
        }

        public static Task<Branch?> CheckoutCommitAsync(Commit commit)
        {
            return Task.Factory.StartNew(() => CheckoutCommit(commit));
        }

        public static RebaseResult? Rebase(Branch branch, Branch onto)
        {
            if (Repository == null)
            {
                return null;
            }

            RebaseOptions options = new();

            try
            {
                return Repository.Rebase.Start(branch, branch.TrackedBranch, onto, identity, options);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to rebase branch!");
                Logger.Log(ex);
                MessageBox.Show("Failed to rebase branch!", ex.Message);
            }
            return null;
        }

        public static Task<RebaseResult?> RebaseAsync(Branch branch, Branch onto)
        {
            return Task.Factory.StartNew(() => Rebase(branch, onto));
        }

        public static void RebaseAbort()
        {
            RebaseOptions options = new();
            Repository?.Rebase.Abort(options);
        }

        public static void RebaseContinue()
        {
            RebaseOptions options = new();
            Repository?.Rebase.Continue(identity, options);
        }

        public static MergeResult? MergeBranch(Branch branch)
        {
            if (Repository == null)
            {
                return null;
            }

            MergeOptions options = new();

            Signature signature = new(identity, DateTimeOffset.Now);

            try
            {
                return Repository.Merge(branch.Tip, signature, options);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to merge branches!");
                Logger.Log(ex);
                MessageBox.Show("Failed to merge branches!", ex.Message);
            }

            return null;
        }

        public static Task<MergeResult?> MergeBranchAsync(Branch branch)
        {
            return Task.Factory.StartNew(() => MergeBranch(branch));
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
                        changedFiles.AddRange(changes.Select(change => $"{(change.Status == ChangeKind.Added ? "+ " : change.Status == ChangeKind.Modified ? "* " : change.Status == ChangeKind.Deleted ? "- " : "")}" + change.Path));
                    }
                });

                filesChanged = false;
            }
            return changedFiles;
        }

        public static void UpdateCommitDifference()
        {
            if (Repository == null)
            {
                return;
            }

            CommitDifference = GetCommitDifference(Repository.Head);
        }

        public static GitCommitDifference GetCommitDifference(Branch branch)
        {
            if (Repository == null)
            {
                return default;
            }

            var remote = branch.TrackedBranch;

            if (remote == null)
            {
                return new GitCommitDifference(0, 0);
            }

            var localTip = branch.Tip;
            var remoteTip = remote.Tip;

            if (localTip == null || remoteTip == null)
            {
                return new GitCommitDifference(0, 0);
            }

            int behindCount = Repository.Commits
                .QueryBy(new CommitFilter
                {
                    IncludeReachableFrom = remoteTip,
                    ExcludeReachableFrom = localTip
                })
                .Count();

            int aheadCount = Repository.Commits
                .QueryBy(new CommitFilter
                {
                    IncludeReachableFrom = localTip,
                    ExcludeReachableFrom = remoteTip
                })
                .Count();

            return new GitCommitDifference(behindCount, aheadCount);
        }

        public static IEnumerable<Branch> GetLocalBranches()
        {
            if (Repository == null)
            {
                yield break;
            }

            foreach (var branch in Repository.Branches)
            {
                if (!branch.IsRemote)
                {
                    yield return branch;
                }
            }
        }

        public static IEnumerable<Branch> GetRemoteBranches()
        {
            if (Repository == null)
            {
                yield break;
            }

            foreach (var branch in Repository.Branches)
            {
                if (branch.IsRemote)
                {
                    yield return branch;
                }
            }
        }
    }

    public readonly struct GitCommitDifference(int behind, int ahead)
    {
        public readonly int Behind = behind;
        public readonly int Ahead = ahead;
        public readonly string Text = $"\xE896 {behind} | \xE898 {ahead}";
    }
}