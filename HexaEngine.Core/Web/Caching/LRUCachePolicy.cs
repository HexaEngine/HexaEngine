namespace HexaEngine.Web.Caching
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a cache policy based on the Least Recently Used (LRU) algorithm.
    /// </summary>
    public unsafe class LRUCachePolicy : ICachePolicy
    {
        /// <summary>
        /// Gets the cache entry that should be removed according to the LRU policy.
        /// </summary>
        /// <param name="entries">The list of cache entries.</param>
        /// <param name="ignore">The cache entry to ignore during selection.</param>
        /// <returns>The cache entry to be removed, or <see langword="null"/> if no entry should be removed.</returns>
        public WebCacheEntry? GetItemToRemove(IList<WebCacheEntry> entries, WebCacheEntry ignore)
        {
            WebCacheEntry? itemToRemove = null;
            DateTime lastAccess = DateTime.MaxValue;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != ignore && entry.Data != null && entry.LastAccess < lastAccess)
                {
                    itemToRemove = entry;
                    lastAccess = entry.LastAccess;
                }
            }
            return itemToRemove;
        }

        /// <summary>
        /// Gets the cache entry that should be removed according to the LRU policy.
        /// </summary>
        /// <param name="entries">The list of cache entries.</param>
        /// <param name="ignore">The cache entry to ignore during selection.</param>
        /// <returns>The cache entry to be removed, or <see langword="null"/> if no entry should be removed.</returns>
        public WebCacheEntry? GetItemToRemoveDisk(IList<WebCacheEntry> entries, WebCacheEntry ignore)
        {
            WebCacheEntry? itemToRemove = null;
            DateTime lastAccess = DateTime.MaxValue;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != ignore && entry.LastAccess < lastAccess)
                {
                    itemToRemove = entry;
                    lastAccess = entry.LastAccess;
                }
            }
            return itemToRemove;
        }
    }
}