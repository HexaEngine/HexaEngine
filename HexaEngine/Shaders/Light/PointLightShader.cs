namespace HexaEngine.Shaders.Light
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources.Buffers;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class PointLightShader : Pipeline
    {
        private readonly IBuffer MatrixBuffer;
        private readonly IBuffer LightBuffer;
        private readonly IBuffer CamBuffer;

        public PointLightShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "point/VertexShader.hlsl",
            PixelShader = "point/PixelShader.hlsl"
        })
        {
            MatrixBuffer = CreateConstantBuffer<ModelViewProj>(ShaderStage.Vertex, 0);
            LightBuffer = CreateConstantBuffer<BufferLightType>(ShaderStage.Pixel, 0);
            CamBuffer = CreateConstantBuffer<CamDescription>(ShaderStage.Pixel, 1);
            State = new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.NonPremultiplied,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct PointLightDescription
        {
            public Vector4 Color;
            public Vector3 Position;
            public float Range;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BufferLightType
        {
            public PointLightDescription LightDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CamDescription
        {
            public Vector3 Position;
            public float reserved;
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        #endregion Structs

        public void Render(IGraphicsContext context, Viewport viewport, IView view, IView scene, int indexCount)
        {
            context.Write(MatrixBuffer, new ModelViewProj(view, Matrix4x4.Identity));

            context.Write(CamBuffer, new CamDescription()
            {
                Position = scene.Transform.Position,
                View = Matrix4x4.Transpose(scene.Transform.View),
                Projection = Matrix4x4.Transpose(scene.Transform.Projection),
            });

            DrawIndexed(context, viewport, view, Matrix4x4.Identity, indexCount, 0, 0);
        }
    }
}