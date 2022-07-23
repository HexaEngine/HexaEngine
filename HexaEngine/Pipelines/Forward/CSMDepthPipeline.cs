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
            };
        }
    }

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
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };
        }
    }
}