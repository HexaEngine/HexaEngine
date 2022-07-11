namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources.Buffers;
    using System.Numerics;

    public class LineShader : Pipeline
    {
        private readonly IBuffer frameBuffer;

        public LineShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "line/VertexShader.hlsl",
            PixelShader = "line/PixelShader.hlsl"
        })
        {
            frameBuffer = CreateConstantBuffer<ModelViewProj>(ShaderStage.Vertex, 0);
            var state = State;
            state.Topology = PrimitiveTopology.LineList;
            State = state;
        }

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform)
        {
            context.Write(frameBuffer, new ModelViewProj(view, transform));
            base.BeginDraw(context, viewport, view, transform);
        }
    }
}