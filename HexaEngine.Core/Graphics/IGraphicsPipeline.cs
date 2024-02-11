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