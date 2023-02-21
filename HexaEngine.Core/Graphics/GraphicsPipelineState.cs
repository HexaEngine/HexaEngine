namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Numerics;

    public struct GraphicsPipelineState
    {
        public RasterizerDescription Rasterizer;
        public DepthStencilDescription DepthStencil;
        public BlendDescription Blend;

        [DefaultValue(PrimitiveTopology.TriangleList)]
        public PrimitiveTopology Topology;

        public Vector4 BlendFactor;

        [DefaultValue(int.MaxValue)]
        public uint SampleMask;

        [DefaultValue(0)]
        public uint StencilRef;

        public static GraphicsPipelineState Default => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleList, BlendFactor = default, SampleMask = int.MaxValue };
    }
}