namespace HexaEngine.Core.IO.Caching
{
    /// <summary>
    /// Represents the persistence state of a cache entry.
    /// </summary>
    public struct CacheEntryPersistenceState
    {
        /// <summary>
        /// Indicates whether the cache entry has been modified and needs to be written back to disk.
        /// </summary>
        public bool IsDirty;

        /// <summary>
        /// The previous size of the cache entry.
        /// </summary>
        public uint OldSize;
    }
}