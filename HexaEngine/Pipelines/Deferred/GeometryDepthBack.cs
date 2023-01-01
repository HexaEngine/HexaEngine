namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;

    public class GeometryDepthBack : GraphicsPipeline
    {
        public IBuffer? Camera;

        public GeometryDepthBack(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/prepass/vs.hlsl",
            HullShader = "deferred/prepass/hs.hlsl",
            DomainShader = "deferred/prepass/ds.hlsl",
            PixelShader = "deferred/prepass/ps.hlsl"
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
            new("DEPTH", 1), new("INSTANCED", 1)
        })
        {
        }

        public override void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            base.BeginDraw(context, viewport);
            context.DSSetConstantBuffer(Camera, 1);
        }
    }
}