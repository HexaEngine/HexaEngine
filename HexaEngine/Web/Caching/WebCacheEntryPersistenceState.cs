namespace HexaEngine.Web.Caching
{
    [Flags]
    public enum WebCacheEntryPersistenceStateFlags
    {
        None = 0,
        Dirty = 1,
        Ghost = 2,
    }

    /// <summary>
    /// Represents the persistence state of a cache entry.
    /// </summary>
    public struct WebCacheEntryPersistenceState
    {
        public WebCacheEntryPersistenceStateFlags Flags;

        /// <summary>
        /// Indicates whether the cache entry has been modified and needs to be written back to disk.
        /// </summary>
        public bool IsDirty
        {
            readonly get => (Flags & WebCacheEntryPersistenceStateFlags.Dirty) != 0; set
            {
                if (value)
                {
                    Flags |= WebCacheEntryPersistenceStateFlags.Dirty;
                }
                else
                {
                    Flags &= ~WebCacheEntryPersistenceStateFlags.Dirty;
                }
            }
        }

        /// <summary>
        /// Indicates whether the cache entry is a ghost entry which means it's not longer in memory nor on disk. Very spooky.
        /// </summary>
        public bool IsGhost
        {
            readonly get => (Flags & WebCacheEntryPersistenceStateFlags.Ghost) != 0; set
            {
                if (value)
                {
                    Flags |= WebCacheEntryPersistenceStateFlags.Ghost;
                }
                else
                {
                    Flags &= ~WebCacheEntryPersistenceStateFlags.Ghost;
                }
            }
        }

        /// <summary>
        /// The previous size of the cache entry.
        /// </summary>
        public uint OldSize;
    }
}