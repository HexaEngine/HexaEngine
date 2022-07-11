namespace HexaEngine.Shaders.Texture
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources.Buffers;
    using System.Numerics;

    public class TextureShader : Pipeline
    {
        private readonly IBuffer MatrixBuffer;

        public TextureShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "texture/VertexShader.hlsl",
            PixelShader = "texture/PixelShader.hlsl"
        })
        {
            MatrixBuffer = CreateConstantBuffer<PerFrameBuffer2>(ShaderStage.Vertex, 0);
            State = PipelineState.Default;
        }

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform)
        {
            context.Write(MatrixBuffer, new PerFrameBuffer2(view, transform));
            base.BeginDraw(context, viewport, view, transform);
        }
    }
}