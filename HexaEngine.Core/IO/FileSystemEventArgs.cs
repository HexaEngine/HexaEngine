namespace HexaEngine.Core.IO
{
    using System;
    using System.IO;

    public class FileSystemEventArgs : EventArgs
    {
        public FileSystemEventArgs(string fullPath, FileSystemChangeTypes changeType)
        {
            Name = Path.GetFileName(fullPath);
            FullPath = fullPath;
            ChangeType = changeType;
        }

        public FileSystemEventArgs(string name, string fullPath, FileSystemChangeTypes changeType)
        {
            Name = name;
            FullPath = fullPath;
            ChangeType = changeType;
        }

        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public FileSystemChangeTypes ChangeType { get; internal set; }
    }
}