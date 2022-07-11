namespace HexaEngine.Shaders.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Resources.Buffers;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class HBAOEffect : Effect
    {
        private readonly IBuffer samplesBuffer;
        private readonly IBuffer mvp;
        private readonly Texture noise;
        public Matrix4x4 Projection;

        public unsafe HBAOEffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/hbao/vs.hlsl",
            PixelShader = "effects/hbao/ps.hlsl",
        })
        {
            AutoClear = true;
            Mesh = new Quad(device);
            mvp = CreateConstantBuffer<ViewProj>(ShaderStage.Pixel, 0);

            Vector4[] samples = new Vector4[64];
            Random random = new();
            for (int i = 0; i < 64; i++)
            {
                samples[i] = new(random.NextSingle(), random.NextSingle(), random.NextSingle(), 0);
            }
            samplesBuffer = CreateConstantBuffer<Vector4>(64, ShaderStage.Pixel, 1);
            device.Context.Write(samplesBuffer, samples);

            Vector4[] pixel = new Vector4[16];
            for (int i = 0; i < 16; i++)
            {
                pixel[i] = new Vector4(random.NextSingle(), random.NextSingle(), 0, 0);
            }

            noise = new(device, MemoryMarshal.AsBytes(pixel.AsSpan()), sizeof(Vector4), TextureDescription.CreateTexture2D(4, 4, 1));
            noise.AddBinding(new(ShaderStage.Pixel, 0));
            Resources.Add(new(noise.ResourceView, ShaderStage.Pixel, 0));
        }

        public override void Draw(IGraphicsContext context, IView view)
        {
            noise.Bind(context, 0);
            context.Write(mvp, new ViewProj(view));
            DrawAuto(context, Target.Viewport, null);
        }

        public override void Dispose()
        {
            noise.Dispose();
            base.Dispose();
        }
    }
}