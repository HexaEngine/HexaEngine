namespace HexaEngine.Shaders.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects.Primitives;

    public class BRDFEffect : Effect
    {
        public BRDFEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/brdf/vs.hlsl",
            PixelShader = "effects/brdf/ps.hlsl"
        })
        {
            AutoClear = true;
            Mesh = new Quad(device);
        }

        public override void Draw(IGraphicsContext context, IView view)
        {
            base.DrawAuto(context, Target.Viewport, null);
        }
    }
}