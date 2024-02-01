namespace HexaEngine.Core.IO.Caching
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a cache policy interface.
    /// </summary>
    public interface ICachePolicy
    {
        /// <summary>
        /// Gets the cache entry that should be removed according to the policy.
        /// </summary>
        /// <param name="entries">The list of cache entries.</param>
        /// <param name="ignore">The cache entry to ignore during selection.</param>
        /// <returns>The cache entry to be removed, or <see langword="null"/> if no entry should be removed.</returns>
        public PersistentCacheEntry? GetItemToRemove(IList<PersistentCacheEntry> entries, PersistentCacheEntry ignore);
    }
}