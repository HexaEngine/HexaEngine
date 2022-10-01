namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class PSMPipeline : Pipeline
    {
        public PSMPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/psm/vs.hlsl",
            HullShader = "forward/psm/hs.hlsl",
            DomainShader = "forward/psm/ds.hlsl",
            PixelShader = "forward/psm/ps.hlsl",
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };
        }
    }
}