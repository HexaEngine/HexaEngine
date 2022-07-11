namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class MTLDepthShaderFront : Pipeline
    {
        public MTLDepthShaderFront(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "material/depth/VertexShader.hlsl",
            HullShader = "material/depth/HullShader.hlsl",
            DomainShader = "material/depth/DomainShader.hlsl",
            PixelShader = "material/depth/PixelShader.hlsl"
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

    public class MTLDepthShaderBack : Pipeline
    {
        public MTLDepthShaderBack(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "material/depth/VertexShader.hlsl",
            HullShader = "material/depth/HullShader.hlsl",
            DomainShader = "material/depth/DomainShader.hlsl",
            PixelShader = "material/depth/PixelShader.hlsl"
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