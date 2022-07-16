namespace HexaEngine.Pipelines
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class MTLDepthShaderFront : Pipeline
    {
        public MTLDepthShaderFront(IGraphicsDevice device) : base(device, new()
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

        protected override ShaderMacro[] GetShaderMacros()
        {
            return new ShaderMacro[] { new("DEPTH", 1) };
        }
    }

    public class MTLDepthShaderBack : Pipeline
    {
        public MTLDepthShaderBack(IGraphicsDevice device) : base(device, new()
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
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };
        }

        protected override ShaderMacro[] GetShaderMacros()
        {
            return new ShaderMacro[] { new("DEPTH", 1) };
        }
    }
}