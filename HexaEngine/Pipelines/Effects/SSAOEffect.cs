namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;

    public class SSAOEffect : Effect
    {
        public unsafe SSAOEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/ssao/vs.hlsl",
            PixelShader = "effects/ssao/ps.hlsl",
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