namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;

    public class PSMPipeline : GraphicsPipeline
    {
        public IBuffer? View;

        public PSMPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/psm/vs.hlsl",
            HullShader = "forward/psm/hs.hlsl",
            DomainShader = "forward/psm/ds.hlsl",
            PixelShader = "forward/psm/ps.hlsl",
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
            context.DSSetConstantBuffer(View, 0);
        }
    }
}