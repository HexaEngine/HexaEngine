namespace HexaEngine.Core.Graphics
{
    using System.Numerics;

    /// <summary>
    /// Interface representing the state of a graphics pipeline.
    /// </summary>
    public interface IGraphicsPipelineState : IPipelineState
    {
        /// <summary>
        /// Gets the pipeline (eg. Vertex Shader .. Pixel Shader) of the graphics pipeline state.
        /// </summary>
        public IGraphicsPipeline Pipeline { get; }

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
    }
}