namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class LightShader : Pipeline
    {
        public LightShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "light/vs.hlsl",
            PixelShader = "light/ps.hlsl",
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };
        }
    }
}