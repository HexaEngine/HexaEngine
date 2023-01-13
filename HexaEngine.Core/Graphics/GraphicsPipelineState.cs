namespace HexaEngine.Core.Graphics
{
    using System.Numerics;

    public struct GraphicsPipelineState
    {
        public RasterizerDescription Rasterizer;
        public DepthStencilDescription DepthStencil;
        public BlendDescription Blend;
        public PrimitiveTopology Topology;
        public Vector4 BlendFactor;
        public uint SampleMask;

        public static GraphicsPipelineState Default => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleList, BlendFactor = default, SampleMask = int.MaxValue };
    }
}