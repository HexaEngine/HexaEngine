namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;

    public class TonemapEffect : Effect
    {
        public TonemapEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/tonemap/vs.hlsl",
            PixelShader = "effects/tonemap/ps.hlsl",
        })
        {
            Mesh = new Quad(device);
            State = new()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        public override void Draw(IGraphicsContext context)
        {
#nullable disable
            DrawAuto(context, Target.Viewport);
#nullable enable
        }

        public void Draw(IGraphicsContext context, Viewport viewport)
        {
            DrawAuto(context, viewport);
        }

        public override void DrawSettings()
        {
        }
    }
}