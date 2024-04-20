namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Provides an interface for profiling GPU performance and timing information.
    /// </summary>
    public interface IGPUProfiler : IDisposable
    {
        /// <summary>
        /// Gets or sets whether the GPU profiler is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets whether logging of GPU profiling information is disabled.
        /// </summary>
        bool DisableLogging { get; set; }

        IReadOnlyList<string> BlockNames { get; }

        /// <summary>
        /// Gets the GPU profiling data for a specific block by name.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <returns>The GPU profiling data for the specified block.</returns>
        double this[string name] { get; }

        /// <summary>
        /// Creates a GPU profiling block with the specified name.
        /// </summary>
        /// <param name="name">The name of the profiling block to create.</param>
        void CreateBlock(string name);

        /// <summary>
        /// Destroys a GPU profiling block with the specified name.
        /// </summary>
        /// <param name="name">The name of the profiling block to destroy.</param>
        void DestroyBlock(string name);

        /// <summary>
        /// Begins a new frame for GPU profiling.
        /// </summary>
        void BeginFrame();

        /// <summary>
        /// Ends the current frame for GPU profiling and provides the associated graphics context.
        /// </summary>
        /// <param name="context">The graphics context associated with the frame.</param>
        void EndFrame(IGraphicsContext context);

        /// <summary>
        /// Begins GPU profiling for a specified block with the given name and associated graphics context.
        /// </summary>
        /// <param name="context">The graphics context associated with the profiling.</param>
        /// <param name="name">The name of the block to begin profiling.</param>
        void Begin(IGraphicsContext context, string name);

        /// <summary>
        /// Ends GPU profiling for a specified block with the given name and associated graphics context.
        /// </summary>
        /// <param name="context">The graphics context associated with the profiling.</param>
        /// <param name="name">The name of the block to end profiling.</param>
        void End(IGraphicsContext context, string name);

        /// <summary>
        /// Ends the current frame for GPU profiling and provides the associated graphics context.
        /// </summary>
        void EndFrame();

        /// <summary>
        /// Begins GPU profiling for a specified block with the given name and associated graphics context.
        /// </summary>
        /// <param name="name">The name of the block to begin profiling.</param>
        void Begin(string name);

        /// <summary>
        /// Ends GPU profiling for a specified block with the given name and associated graphics context.
        /// </summary>
        /// <param name="name">The name of the block to end profiling.</param>
        void End(string name);
    }
}