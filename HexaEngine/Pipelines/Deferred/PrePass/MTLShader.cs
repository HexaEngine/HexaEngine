namespace HexaEngine.Pipelines.Deferred.PrePass
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class MTLShader : Pipeline
    {
        public MTLShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "deferred/prepass/vs.hlsl",
            HullShader = "deferred/prepass/hs.hlsl",
            DomainShader = "deferred/prepass/ds.hlsl",
            PixelShader = "deferred/prepass/ps.hlsl"
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