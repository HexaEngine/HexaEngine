namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;

    public class BlendEffect : Effect
    {
        public BlendEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/blend/vs.hlsl",
            PixelShader = "effects/blend/ps.hlsl",
        })
        {
            Mesh = new Quad(device);
            State = new()
            {
                DepthStencil = DepthStencilDescription.None,
                Blend = BlendDescription.NonPremultiplied,
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

        public override void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}