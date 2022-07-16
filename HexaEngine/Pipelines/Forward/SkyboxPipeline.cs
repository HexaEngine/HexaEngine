namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class SkyboxPipeline : Pipeline
    {
        public SkyboxPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/skybox/vs.hlsl",
            PixelShader = "forward/skybox/ps.hlsl"
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };
        }
    }
}