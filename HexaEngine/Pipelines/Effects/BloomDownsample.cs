namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;

    public class BloomDownsample : Effect
    {
        public BloomDownsample(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/bloom/downsample/vs.hlsl",
            PixelShader = "effects/bloom/downsample/ps.hlsl",
        })
        {
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
    }
}