namespace HexaEngine.Graphics.Graph
{
    using System;

    /// <summary>
    /// Flags to control resource creation.
    /// </summary>
    [Flags]
    public enum ResourceCreationFlags
    {
        /// <summary>
        /// No special flags set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Resources are lazily initialized. This means the resource is not immediately available, but is guaranteed available after the graph has been built.
        /// </summary>
        LazyInit = 1,

        /// <summary>
        /// Resources are shared and can be used by multiple effects/passes.
        /// Note: Shared resources require LazyInit to be set.
        /// </summary>
        Shared = 2,

        /// <summary>
        /// Allows a resource to be shared in a single <see cref="GraphResourceContainer"/>.
        /// </summary>
        GroupShared = 4,

        /// <summary>
        /// Enable all flags.
        /// </summary>
        All = LazyInit | Shared,
    }
}