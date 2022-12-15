namespace HexaEngine.Pipelines.Deferred;

using HexaEngine.Core.Graphics;
using HexaEngine.Graphics;

public class SkinnedMTLShader : Pipeline
{
    public SkinnedMTLShader(IGraphicsDevice device) : base(device, new()
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