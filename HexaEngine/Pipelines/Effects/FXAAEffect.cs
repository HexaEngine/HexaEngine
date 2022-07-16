namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;

    public class FXAAEffect : Effect
    {
        public FXAAEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/fxaa/vs.hlsl",
            PixelShader = "effects/fxaa/ps.hlsl"
        })
        {
            AutoClear = true;
            Mesh = new Quad(device);
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
            throw new NotImplementedException();
        }
    }
}