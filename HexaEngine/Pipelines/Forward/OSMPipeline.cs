namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public unsafe class OSMPipeline : GraphicsPipeline
    {
        public ConstantBuffer<Matrix4x4>? View;
        public IBuffer? Light;

        public OSMPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/osm/vs.hlsl",
            HullShader = "forward/osm/hs.hlsl",
            DomainShader = "forward/osm/ds.hlsl",
            GeometryShader = "forward/osm/gs.hlsl",
            PixelShader = "forward/osm/ps.hlsl",
        },
        new GraphicsPipelineState()
        {
            DepthStencil = DepthStencilDescription.Default,
            Rasterizer = RasterizerDescription.CullFront,
            Blend = BlendDescription.Opaque,
            Topology = PrimitiveTopology.PatchListWith3ControlPoints,
        },
        new ShaderMacro[]
        {
            new("INSTANCED", 1)
        })
        {
        }

        public override void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            base.BeginDraw(context, viewport);
            context.GSSetConstantBuffer(View?.Buffer, 0);
            context.PSSetConstantBuffer(Light, 0);
        }
    }
}