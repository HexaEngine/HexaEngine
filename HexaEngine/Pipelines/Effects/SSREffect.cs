namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;

    public class SSREffect : Effect
    {
        public SSREffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/ssr/vs.hlsl",
            PixelShader = "effects/ssr/ps.hlsl",
        })
        {
            Mesh = new Quad(device);
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