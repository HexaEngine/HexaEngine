namespace HexaEngine.Pipelines.Deferred.Lighting
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    [Obsolete("Convert to effect for easy usage")]
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