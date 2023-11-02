namespace HexaEngine.Core.IO
{
    /// <summary>
    /// Specifies the type of file system change that occurred.
    /// </summary>
    public enum FileSystemChangeTypes
    {
        /// <summary>
        /// Indicates that a file or directory was created.
        /// </summary>
        Created,

        /// <summary>
        /// Indicates that a file or directory was deleted.
        /// </summary>
        Deleted,

        /// <summary>
        /// Indicates that a file or directory was changed.
        /// </summary>
        Changed,

        /// <summary>
        /// Indicates that a file or directory was renamed.
        /// </summary>
        Renamed
    }
}