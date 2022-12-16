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
        new InputElementDescription[]
        {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("COLOR", 0, Format.RGBA32Float, 0),
                new("INSTANCED_MATS", 0, Format.RGBA32Float, 0, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 1, Format.RGBA32Float, 16, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 2, Format.RGBA32Float, 32, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 3, Format.RGBA32Float, 48, 1, InputClassification.PerInstanceData, 1),
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
        new InputElementDescription[]
        {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXTURE", 0, Format.RG32Float, 0),
                new("TEXTURE", 1, Format.RG32Float, 0),
                new("INSTANCED_MATS", 0, Format.RGBA32Float, 0, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 1, Format.RGBA32Float, 16, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 2, Format.RGBA32Float, 32, 1, InputClassification.PerInstanceData, 1),
                new("INSTANCED_MATS", 3, Format.RGBA32Float, 48, 1, InputClassification.PerInstanceData, 1),
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