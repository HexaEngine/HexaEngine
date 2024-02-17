namespace HexaEngine.Core.Graphics
{
    using System.Numerics;

    /// <summary>
    /// Interface representing the state of a graphics pipeline.
    /// </summary>
    public interface IGraphicsPipelineState : IDisposable
    {
        /// <summary>
        /// Gets the pipeline (eg. Vertex Shader .. Pixel Shader) of the graphics pipeline state.
        /// </summary>
        public IGraphicsPipeline Pipeline { get; }

        /// <summary>
        /// Gets the binding list of the graphics pipeline state.
        /// </summary>
        public IResourceBindingList Bindings { get; }

        /// <summary>
        /// Gets the description of the graphics pipeline state.
        /// </summary>
        GraphicsPipelineStateDesc Description { get; }

        /// <summary>
        /// Gets or sets the primitive topology of the graphics pipeline state.
        /// </summary>
        PrimitiveTopology Topology { get; set; }

        /// <summary>
        /// Gets or sets the blend factor of the graphics pipeline state.
        /// </summary>
        Vector4 BlendFactor { get; set; }

        /// <summary>
        /// Gets or sets the stencil ref of the graphics pipeline state.
        /// </summary>
        uint StencilRef { get; set; }

        /// <summary>
        /// Gets a value indicating whether the graphics pipeline is initialized and ready for use.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets a value indicating whether the graphics pipeline is valid and can be used for rendering.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether the graphics pipeline is disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets a value indicating whether the graphics pipeline is ready for use, taking both initialization and validity into account.
        /// </summary>
        bool IsReady => IsInitialized && IsValid && !IsDisposed;
    }
}