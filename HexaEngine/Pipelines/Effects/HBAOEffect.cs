namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;

    public class HBAOEffect : Effect
    {
        public unsafe HBAOEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/hbao/vs.hlsl",
            PixelShader = "effects/hbao/ps.hlsl",
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