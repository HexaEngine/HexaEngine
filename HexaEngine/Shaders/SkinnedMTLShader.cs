namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Resources.Buffers;
    using System.Numerics;

    public class SkinnedMTLShader : Pipeline
    {
        private readonly IBuffer MatrixBuffer;
        private readonly IBuffer MaterialBuffer;

        public SkinnedMTLShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "material/VertexShader.hlsl",
            HullShader = "material/HullShader.hlsl",
            DomainShader = "material/DomainShader.hlsl",
            PixelShader = "material/PixelShader.hlsl"
        })
        {
            MatrixBuffer = CreateConstantBuffer<ModelViewProj>(ShaderStage.Domain, 0);
            MaterialBuffer = CreateConstantBuffer<CBMaterial>(new ShaderBinding(ShaderStage.Pixel, 0), new ShaderBinding(ShaderStage.Domain, 1));
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.NonPremultiplied,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            };
        }

        public Material Material;

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform)
        {
            context.Write(MatrixBuffer, new ModelViewProj(view, transform));
            context.Write(MaterialBuffer, (CBMaterial)Material);
            base.BeginDraw(context, viewport, view, transform);
        }
    }
}