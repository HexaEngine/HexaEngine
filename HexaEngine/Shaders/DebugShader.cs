namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class DebugShader : Pipeline
    {
        private readonly IBuffer DebugBuffer;
        public DebugMode Mode;

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct DebugMode
        {
            public int Filter;
            public Vector3 padd0;
            public int Mode;
            public Vector3 padd1;
        }

        #endregion Structs

        public DebugShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "debugging/vs.hlsl",
            PixelShader = "debugging/ps.hlsl",
        })
        {
            DebugBuffer = CreateConstantBuffer<DebugMode>(ShaderStage.Pixel, 0);
            State = new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.NonPremultiplied,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform)
        {
            context.Write(DebugBuffer, Mode);
            base.BeginDraw(context, viewport, view, transform);
        }
    }
}