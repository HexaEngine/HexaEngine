namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;

    public class CSMPipeline : GraphicsPipeline
    {
        public IBuffer? View;

        public CSMPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/csm/vs.hlsl",
            HullShader = "forward/csm/hs.hlsl",
            DomainShader = "forward/csm/ds.hlsl",
            GeometryShader = "forward/csm/gs.hlsl",
            PixelShader = "forward/csm/ps.hlsl",
        },
        new ShaderMacro[]
        {
            new("INSTANCED", 1)
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };
        }

        public override void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            base.BeginDraw(context, viewport);
            context.GSSetConstantBuffer(View, 0);
        }
    }
}