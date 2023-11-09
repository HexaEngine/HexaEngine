namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a graphics pipeline that specifies the rendering state and shaders for graphics rendering.
    /// </summary>
    public interface IGraphicsPipeline : IDisposable
    {
        /// <summary>
        /// Gets the description of the graphics pipeline, including shader information and other state settings.
        /// </summary>
        GraphicsPipelineDesc Description { get; }

        /// <summary>
        /// Gets the name used for debugging and identification of the graphics pipeline.
        /// </summary>
        string DebugName { get; }

        /// <summary>
        /// Gets the current state of the graphics pipeline, which includes rendering settings.
        /// </summary>
        GraphicsPipelineState State { get; }

        /// <summary>
        /// Begins a drawing operation with the specified graphics context using this graphics pipeline.
        /// </summary>
        /// <param name="context">The graphics context to begin drawing with.</param>
        void BeginDraw(IGraphicsContext context);

        /// <summary>
        /// Ends a drawing operation with the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to end the drawing operation with.</param>
        void EndDraw(IGraphicsContext context);

        /// <summary>
        /// Recompiles the graphics pipeline, updating its shaders and state settings.
        /// </summary>
        void Recompile();

        /// <summary>
        /// Gets or sets an array of shader macros for customizing shader compilation.
        /// </summary>
        ShaderMacro[]? Macros { get; set; }

        /// <summary>
        /// Gets a value indicating whether the graphics pipeline is initialized and ready for use.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets a value indicating whether the graphics pipeline is valid and can be used for rendering.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether the graphics pipeline is ready for use, taking both initialization and validity into account.
        /// </summary>
        bool IsReady => IsInitialized && IsValid;
    }
}