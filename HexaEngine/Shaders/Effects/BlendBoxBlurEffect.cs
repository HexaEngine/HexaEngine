namespace HexaEngine.Shaders.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;

    public class BlendBoxBlurEffect : Effect
    {
        private readonly IBuffer paramsBuffer;
        private int size;

        private struct Params
        {
            public int Size;
            public Vector3 Padd;

            public Params()
            {
                Size = 1;
                Padd = new();
            }
        }

        public BlendBoxBlurEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/blur/vs.hlsl",
            PixelShader = "effects/blur/box.hlsl",
        })
        {
            paramsBuffer = CreateConstantBuffer<Params>(ShaderStage.Pixel, 0);
            Size = 2;
            Mesh = new Quad(device);
            State = new()
            {
                Blend = BlendDescription.NonPremultiplied,
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        public int Size
        {
            get => size;
            set { size = value; }
        }

        public override void Draw(IGraphicsContext context, IView view)
        {
            context.Write(paramsBuffer, new Params() { Size = size });
            DrawAuto(context, default, null);
        }
    }

    public class BlendGaussianBlurEffect : Effect
    {
        private readonly IBuffer paramsBuffer;
        private int size;

        private struct Params
        {
            public int Size;
            public Vector3 Padd;

            public Params()
            {
                Size = 1;
                Padd = new();
            }
        }

        public BlendGaussianBlurEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/blur/vs.hlsl",
            PixelShader = "effects/blur/gaussian.hlsl",
        })
        {
            paramsBuffer = CreateConstantBuffer<Params>(ShaderStage.Pixel, 0);
            Size = 2;
            Mesh = new Quad(device);
            State = new()
            {
                Blend = BlendDescription.NonPremultiplied,
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
            };
        }

        public int Size
        {
            get => size;
            set { size = value; }
        }

        public override void Draw(IGraphicsContext context, IView view)
        {
            context.Write(paramsBuffer, new Params() { Size = size });
            DrawAuto(context, default, null);
        }
    }
}