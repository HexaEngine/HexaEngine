namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;

    public unsafe class Tonemap : IEffect
    {
        private readonly Quad quad;
        private readonly Pipeline pipeline;
        private readonly IBuffer buffer;
        private readonly ISamplerState sampler;
        private readonly void** srvs;
        private float bloomStrength = 0.04f;
        private bool dirty;

        public IRenderTargetView? Output;
        public IShaderResourceView? HDR;
        public IShaderResourceView? Bloom;

        private struct Params
        {
            public float BloomStrength;
            public Vector3 padd;

            public Params(float bloomStrength)
            {
                BloomStrength = bloomStrength;
                padd = default;
            }
        }

        public Tonemap(IGraphicsDevice device)
        {
            quad = new(device);
            pipeline = new(device, new()
            {
                VertexShader = "effects/tonemap/vs.hlsl",
                PixelShader = "effects/tonemap/ps.hlsl",
            });
            buffer = device.CreateBuffer(new Params(bloomStrength), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);
            srvs = AllocArray(2);
        }

        public float BloomStrength
        { get => bloomStrength; set { bloomStrength = value; dirty = true; } }

        public void Resize()
        {
            srvs[0] = (void*)HDR?.NativePointer;
            srvs[1] = (void*)Bloom?.NativePointer;
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output is null) return;
            if (dirty)
            {
                var data = new Params(BloomStrength);
                context.Write(buffer, &data, sizeof(Params));
                dirty = false;
            }

            context.SetRenderTarget(Output, default);
            context.PSSetConstantBuffer(buffer, 0);
            context.PSSetSampler(sampler, 0);
            context.PSSetShaderResources(srvs, 2, 0);
            quad.DrawAuto(context, pipeline, Output.Viewport);
        }

        public void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
            buffer.Dispose();
            Free(srvs);
            GC.SuppressFinalize(this);
        }
    }
}