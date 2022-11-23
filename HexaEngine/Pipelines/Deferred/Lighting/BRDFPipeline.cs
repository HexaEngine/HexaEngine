namespace HexaEngine.Pipelines.Deferred.Lighting
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class BRDFPipeline : Pipeline
    {
        public BRDFPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/pbrbrdf/vs.hlsl",
            PixelShader = "deferred/pbrbrdf/ps.hlsl",
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