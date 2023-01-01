namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class Grid : GraphicsPipeline
    {
        public Grid(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/grid/vs.hlsl",
            PixelShader = "deferred/grid/ps.hlsl"
        },
        new ShaderMacro[]
        {
            new("INSTANCED", 1)
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.LineList,
            };
        }
    }

    public class Terrain : GraphicsPipeline
    {
        public Terrain(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/terrain/vs.hlsl",
            HullShader = "deferred/terrain/hs.hlsl",
            DomainShader = "deferred/terrain/ds.hlsl",
            PixelShader = "deferred/terrain/ps.hlsl"
        },
        new ShaderMacro[]
        {
            new("INSTANCED", 1)
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