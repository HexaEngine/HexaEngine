namespace HexaEngine.Shaders
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct CamDescription
    {
        public Vector3 Position;
        public float reserved;
        public Matrix4x4 View;
        public Matrix4x4 Projection;

        public CamDescription(Vector3 position, Matrix4x4 view, Matrix4x4 projection)
        {
            Position = position;
            reserved = 0;
            View = Matrix4x4.Transpose(view);
            Projection = Matrix4x4.Transpose(projection);
        }

        public CamDescription(CameraTransform transform)
        {
            Position = transform.Position;
            reserved = 0;
            View = Matrix4x4.Transpose(transform.View);
            Projection = Matrix4x4.Transpose(transform.Projection);
        }
    }

    public class DeferredComposeShader : Pipeline
    {
        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct DirectionalLightDescription
        {
            public Vector4 Color;
            public Vector3 LightDirection;
            public float reserved;
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BufferLightType
        {
            public DirectionalLightDescription LightDescription;
        }

        #endregion Structs

        public DeferredComposeShader(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "compose/vs.hlsl",
            PixelShader = "compose/ps.hlsl",
        })
        {
            State = new()
            {
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.NonPremultiplied,
                Topology = PrimitiveTopology.TriangleList,
            };
        }
    }
}