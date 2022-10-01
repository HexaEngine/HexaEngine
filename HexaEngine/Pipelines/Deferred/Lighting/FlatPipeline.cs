namespace HexaEngine.Pipelines.Deferred.Lighting
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class FlatPipeline : Pipeline
    {
        public FlatPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/flat/vs.hlsl",
            PixelShader = "deferred/flat/ps.hlsl",
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Additive,
                Topology = PrimitiveTopology.TriangleList,
            };
        }
    }
}