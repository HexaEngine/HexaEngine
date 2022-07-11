namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources.Buffers;
    using System.Numerics;

    public class SkyboxShader : Pipeline
    {
        private readonly IBuffer mvp;

        public SkyboxShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "skybox/vs.hlsl",
            PixelShader = "skybox/ps.hlsl"
        })
        {
            mvp = CreateConstantBuffer<ModelViewProj>(ShaderStage.Vertex, 0);
            State = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform)
        {
            context.Write(mvp, new ModelViewProj(view, Matrix4x4.CreateScale(view.Transform.Far) * Matrix4x4.CreateTranslation(view.Transform.Position)));
            base.BeginDraw(context, viewport, view, transform);
        }
    }
}