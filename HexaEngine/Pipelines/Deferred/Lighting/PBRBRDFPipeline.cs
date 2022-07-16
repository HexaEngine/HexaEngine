namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class PBRBRDFPipeline : Pipeline
    {
        public PBRBRDFPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/pbrbrdf/vs.hlsl",
            PixelShader = "deferred/pbrbrdf/ps.hlsl",
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