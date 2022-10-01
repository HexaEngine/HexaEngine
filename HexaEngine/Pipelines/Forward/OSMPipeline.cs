namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class OSMPipeline : Pipeline
    {
        public OSMPipeline(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "forward/osm/vs.hlsl",
            HullShader = "forward/osm/hs.hlsl",
            DomainShader = "forward/osm/ds.hlsl",
            GeometryShader = "forward/osm/gs.hlsl",
            PixelShader = "forward/osm/ps.hlsl",
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };
        }
    }
}