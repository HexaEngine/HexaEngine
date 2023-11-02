namespace HexaEngine.Core.IO
{
    using System.IO;

    /// <summary>
    /// Provides data for file system events involving renamed files or directories.
    /// </summary>
    public class RenamedEventArgs : FileSystemEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenamedEventArgs"/> class
        /// with the old full path and the new full path.
        /// </summary>
        /// <param name="oldFullPath">The old full path of the renamed file or directory.</param>
        /// <param name="fullPath">The new full path of the renamed file or directory.</param>
        public RenamedEventArgs(string oldFullPath, string fullPath) : base(fullPath, FileSystemChangeTypes.Renamed)
        {
            OldName = Path.GetFileName(oldFullPath);
            OldFullPath = oldFullPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenamedEventArgs"/> class
        /// with the old name, old full path, new name, and new full path.
        /// </summary>
        /// <param name="oldName">The old name of the renamed file or directory.</param>
        /// <param name="oldFullPath">The old full path of the renamed file or directory.</param>
        /// <param name="name">The new name of the renamed file or directory.</param>
        /// <param name="fullPath">The new full path of the renamed file or directory.</param>
        public RenamedEventArgs(string oldName, string oldFullPath, string name, string fullPath) : base(name, fullPath, FileSystemChangeTypes.Renamed)
        {
            OldName = oldName;
            OldFullPath = oldFullPath;
        }

        /// <summary>
        /// Gets the old name of the renamed file or directory.
        /// </summary>
        public string OldName { get; internal set; }

        /// <summary>
        /// Gets the old full path of the renamed file or directory.
        /// </summary>
        public string OldFullPath { get; internal set; }
    }
}