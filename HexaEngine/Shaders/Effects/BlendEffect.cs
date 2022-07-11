namespace HexaEngine.Shaders.Effects
{
    using HexaEngine.Core.Graphics;
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

        public override void Draw(IGraphicsContext context, IView view)
        {
            DrawAuto(context, Target.Viewport, null);
        }
    }
}