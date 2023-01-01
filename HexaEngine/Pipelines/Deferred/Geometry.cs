namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;

    public class Geometry : GraphicsPipeline
    {
        public IBuffer? Camera;

        public Geometry(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/prepass/vs.hlsl",
            HullShader = "deferred/prepass/hs.hlsl",
            DomainShader = "deferred/prepass/ds.hlsl",
            PixelShader = "deferred/prepass/ps.hlsl"
        },
        new GraphicsPipelineState()
        {
            DepthStencil = DepthStencilDescription.Default,
            Rasterizer = RasterizerDescription.CullBack,
            Blend = BlendDescription.Opaque,
            Topology = PrimitiveTopology.PatchListWith3ControlPoints,
        },
        new InputElementDescription[]
        {
            new("POSITION", 0, Format.RGB32Float, 0),
            new("TEXCOORD", 0, Format.RG32Float, 0),
            new("NORMAL", 0, Format.RGB32Float, 0),
            new("TANGENT", 0, Format.RGB32Float, 0),
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
            context.DSSetConstantBuffer(Camera, 1);
        }

        public void BeginDrawDepth(IGraphicsContext context, Viewport viewport)
        {
            context.DSSetConstantBuffer(Camera, 1);

            context.VSSetShader(vs);
            context.HSSetShader(hs);
            context.DSSetShader(ds);
            context.GSSetShader(gs);
            context.PSSetShader(null);

            context.SetViewport(viewport);
            context.SetRasterizerState(rasterizerState);
            context.SetBlendState(blendState);
            context.SetDepthStencilState(depthStencilState);
            context.SetInputLayout(layout);
            context.SetPrimitiveTopology(state.Topology);
        }
    }
}