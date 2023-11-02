namespace HexaEngine.Core.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// Provides data for file system events.
    /// </summary>
    public class FileSystemEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemEventArgs"/> class
        /// with the full path and the change type.
        /// </summary>
        /// <param name="fullPath">The full path of the file or directory involved in the event.</param>
        /// <param name="changeType">The type of file system change that occurred.</param>
        public FileSystemEventArgs(string fullPath, FileSystemChangeTypes changeType)
        {
            Name = Path.GetFileName(fullPath);
            FullPath = fullPath;
            ChangeType = changeType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemEventArgs"/> class
        /// with the name, full path, and the change type.
        /// </summary>
        /// <param name="name">The name of the file or directory involved in the event.</param>
        /// <param name="fullPath">The full path of the file or directory involved in the event.</param>
        /// <param name="changeType">The type of file system change that occurred.</param>
        public FileSystemEventArgs(string name, string fullPath, FileSystemChangeTypes changeType)
        {
            Name = name;
            FullPath = fullPath;
            ChangeType = changeType;
        }

        /// <summary>
        /// Gets the name of the file or directory involved in the event.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the full path of the file or directory involved in the event.
        /// </summary>
        public string FullPath { get; internal set; }

        /// <summary>
        /// Gets the type of file system change that occurred.
        /// </summary>
        public FileSystemChangeTypes ChangeType { get; internal set; }
    }
}