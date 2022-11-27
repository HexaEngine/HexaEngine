namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;

    public class BoxBlurEffect : Effect
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

        public BoxBlurEffect(IGraphicsDevice device) : base(device, new()
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
                Blend = BlendDescription.Opaque,
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

        public override unsafe void Draw(IGraphicsContext context)
        {
            var p = new Params() { Size = size };
            context.Write(paramsBuffer, &p, sizeof(Params));
            DrawAuto(context, default);
        }

        public override void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}