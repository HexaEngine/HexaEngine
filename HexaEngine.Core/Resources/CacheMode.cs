namespace HexaEngine.Core.Resources
{
    /// <summary>
    /// Specifies the caching mode for a cache system.
    /// </summary>
    public enum CacheMode
    {
        /// <summary>
        /// Data is read directly from the source and bypasses the cache.
        /// </summary>
        ReadThough,

        /// <summary>
        /// Data is preloaded into the cache from the source before it is requested.
        /// </summary>
        PreLoad,

        /// <summary>
        /// Data is only loaded into the cache when requested, and not preloaded.
        /// </summary>
        OnDemand,
    }
}