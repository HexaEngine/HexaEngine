namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Interface representing the state of a graphics pipeline.
    /// </summary>
    public interface IGraphicsPipelineState
    {
        /// <summary>
        /// Gets the description of the graphics pipeline state.
        /// </summary>
        GraphicsPipelineState Description { get; }

        /// <summary>
        /// Gets or sets the primitive topology of the graphics pipeline state.
        /// </summary>
        PrimitiveTopology Topology { get; set; }
    }
}