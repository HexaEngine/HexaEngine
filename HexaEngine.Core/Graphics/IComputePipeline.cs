namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a compute pipeline used for running compute shaders on a graphics device.
    /// </summary>
    public interface IComputePipeline : IDisposable
    {
        /// <summary>
        /// Gets the debug name of the compute pipeline.
        /// </summary>
        string DebugName { get; }

        /// <summary>
        /// Begins a dispatch operation using the compute pipeline.
        /// </summary>
        /// <param name="context">The graphics context to use for the dispatch operation.</param>
        void BeginDispatch(IGraphicsContext context);

        /// <summary>
        /// Dispatches a compute shader using the compute pipeline with the specified thread group dimensions.
        /// </summary>
        /// <param name="context">The graphics context to use for the dispatch operation.</param>
        /// <param name="x">The number of thread groups in the X dimension.</param>
        /// <param name="y">The number of thread groups in the Y dimension.</param>
        /// <param name="z">The number of thread groups in the Z dimension.</param>
        void Dispatch(IGraphicsContext context, uint x, uint y, uint z);

        /// <summary>
        /// Ends the current dispatch operation using the compute pipeline.
        /// </summary>
        /// <param name="context">The graphics context to use for the dispatch operation.</param>
        void EndDispatch(IGraphicsContext context);

        /// <summary>
        /// Recompiles the compute pipeline.
        /// </summary>
        void Recompile();

        /// <summary>
        /// Gets or sets an array of shader macros used for shader compilation.
        /// </summary>
        ShaderMacro[]? Macros { get; set; }

        /// <summary>
        /// Gets the description of the compute pipeline.
        /// </summary>
        ComputePipelineDesc Desc { get; }

        /// <summary>
        /// Gets a value indicating whether the compute pipeline has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets a value indicating whether the compute pipeline is valid and can be used.
        /// </summary>
        bool IsValid { get; }
    }
}