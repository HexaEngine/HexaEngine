namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;

    public class BRDFLUT : Effect
    {
        public BRDFLUT(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/brdf/vs.hlsl",
            PixelShader = "effects/brdf/ps.hlsl"
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

        public override void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}