namespace HexaEngine.Core.IO
{
    public class LooseFileSystemLayer : FileSystemLayer
    {
        private readonly List<string> sources = [];
        private readonly Dictionary<string, string> fileIndices = new();
        private readonly HashSet<string> fileIndicesHashes = new();
        private readonly List<FileSystemWatcher> watchers = new();
        public override void Initialize()
        {
            Refresh();
        }

        private void Refresh()
        {
            fileIndices.Clear();
            fileIndicesHashes.Clear();
            IndexSource("assets\\");

            for (int i = 0; i < sources.Count; i++)
            {
                IndexSource(sources[i]);
            }
        }

        private void IndexSource(string source)
        {
            string[] directories = Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly);
            for (int j = 0; j < directories.Length; j++)
            {
                string dir = directories[j];
                string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                for (int ii = 0; ii < files.Length; ii++)
                {
                    string file = files[ii];
                    var abs = Path.GetFullPath(file);
                    var rel = Path.GetRelativePath(source, abs);
                    var log = Path.Combine("assets\\", rel);

                    if (!fileIndices.TryAdd(log, abs))
                    {
                        fileIndices[log] = abs;
                    }
                    else
                    {
                        fileIndicesHashes.Add(log);
                    }
                }
            }
        }

        private void AddFileSystemWatcher(string path)
        {
            FileSystemWatcher watcher = new(path);
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += FileSystemWatcherChanged;
            watchers.Add(watcher);
        }

        private void RemoveFileSystemWatcher(string path)
        {
            for (int i = 0; i < watchers.Count; i++)
            {
                var watcher = watchers[i];
                if (watcher.Path == path)
                {
                    watcher.Changed -= FileSystemWatcherChanged;
                    watcher.Dispose();
                    watchers.RemoveAt(i);
                    return;
                }
            }
        }

        public void AddSource(string source)
        {
            if (!sources.Contains(source))
            {
                sources.Add(source);
                IndexSource(source);
                AddFileSystemWatcher(source);
            }
        }

        public void RemoveSource(string source)
        {
            if (sources.Remove(source))
            {
                RemoveFileSystemWatcher(source);
                Refresh();
            }
        }

        private void FileSystemWatcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            OnFileChanged(e);
        }

        public override VirtualStream? Create(in AssetPath path)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(in AssetPath path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetFiles(in AssetPath path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetFiles(in AssetPath path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        public override string GetFullPath(in AssetPath path)
        {
            throw new NotImplementedException();
        }

        public override VirtualStream? OpenRead(in AssetPath path)
        {
            throw new NotImplementedException();
        }

        public override VirtualStream? OpenWrite(in AssetPath path)
        {
            throw new NotImplementedException();
        }

        protected override void DisposeCore()
        {
            throw new NotImplementedException();
        }
    }
}
