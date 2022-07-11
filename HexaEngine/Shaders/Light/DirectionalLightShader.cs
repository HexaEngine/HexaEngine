namespace HexaEngine.Shaders.Light
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class DirectionalLightShader : Pipeline
    {
        public DirectionalLightShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "directional/vs.hlsl",
            PixelShader = "directional/ps.hlsl",
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