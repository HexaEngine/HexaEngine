namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;

    public struct PipelineState
    {
        public RasterizerDescription Rasterizer;
        public DepthStencilDescription DepthStencil;
        public BlendDescription Blend;
        public PrimitiveTopology Topology;

        public static PipelineState Default => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleList };
    }
}