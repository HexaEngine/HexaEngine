namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class CSMPipeline : Pipeline
    {
        public CSMPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/csm/vs.hlsl",
            HullShader = "forward/csm/hs.hlsl",
            DomainShader = "forward/csm/ds.hlsl",
            GeometryShader = "forward/csm/gs.hlsl",
            PixelShader = "forward/csm/ps.hlsl",
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
                //Topology = PrimitiveTopology.TriangleList,
            };
        }
    }
}