namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class MTLShader : Pipeline
    {
        public MTLShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "material/VertexShader.hlsl",
            HullShader = "material/HullShader.hlsl",
            DomainShader = "material/DomainShader.hlsl",
            PixelShader = "material/PixelShader.hlsl"
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