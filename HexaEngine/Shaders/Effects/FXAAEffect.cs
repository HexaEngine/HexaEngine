namespace HexaEngine.Shaders.Effects
{
    using HexaEngine.Core.Graphics;
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

        public override void Draw(IGraphicsContext context, IView view)
        {
            DrawAuto(context, Target.Viewport, null);
        }

        public void Draw(IGraphicsContext context, Viewport viewport)
        {
            DrawAuto(context, viewport, null);
        }
    }
}