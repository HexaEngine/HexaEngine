namespace HexaEngine.Core.IO
{
    using System.IO;

    public class RenamedEventArgs : FileSystemEventArgs
    {
        public RenamedEventArgs(string oldFullPath, string fullPath) : base(fullPath, FileSystemChangeTypes.Renamed)
        {
            OldName = Path.GetFileName(oldFullPath);
            OldFullPath = oldFullPath;
        }

        public RenamedEventArgs(string oldName, string oldFullPath, string name, string fullPath) : base(name, fullPath, FileSystemChangeTypes.Renamed)
        {
            OldName = oldName;
            OldFullPath = oldFullPath;
        }

        public string OldName { get; internal set; }

        public string OldFullPath { get; internal set; }
    }
}