namespace HexaEngine.Core.IO
{
    public abstract class FileSystemLayer : DisposableBase
    {
        public event Action<FileSystemLayer, System.IO.FileSystemEventArgs>? FileChanged;

        public abstract void Initialize();

        public abstract bool Exists(in AssetPath path);

        public abstract VirtualStream? OpenRead(in AssetPath path);

        public abstract VirtualStream? OpenWrite(in AssetPath path);

        public abstract VirtualStream? Create(in AssetPath path);

        public abstract IEnumerable<string> GetFiles(in AssetPath path);

        public abstract IEnumerable<string> GetFiles(in AssetPath path, string searchPattern);

        public abstract string GetFullPath(in AssetPath path);

        protected void OnFileChanged(System.IO.FileSystemEventArgs args)
        {
            FileChanged?.Invoke(this, args);
        }
    }
}
