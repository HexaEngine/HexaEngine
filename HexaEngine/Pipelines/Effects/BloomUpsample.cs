namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;

    public class BloomUpsample : Effect
    {
        public BloomUpsample(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/bloom/upsample/vs.hlsl",
            PixelShader = "effects/bloom/upsample/ps.hlsl",
        })
        {
            State = new()
            {
                Blend = BlendDescription.Additive,
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
            };
            Mesh = new Quad(device);
            AutoSetTarget = false;
        }

        public override void Draw(IGraphicsContext context)
        {
#nullable disable
            DrawAuto(context, Target.Viewport);
#nullable enable
        }

        public void Draw(IGraphicsContext context, Viewport viewport)
        {
#nullable disable
            DrawAuto(context, viewport);
#nullable enable
        }

        public override void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}