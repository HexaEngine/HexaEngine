#nullable disable

namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using System.Numerics;

    public class Tonemap : IEffect
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private IBuffer buffer;
        private ISamplerState sampler;
        private unsafe void** srvs;
        private float bloomStrength = 0.04f;
        private bool dirty;

        private IRenderTargetView Output;
        private IShaderResourceView HDR;
        private IShaderResourceView Bloom;

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

        public float BloomStrength
        { get => bloomStrength; set { bloomStrength = value; dirty = true; } }

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            HDR = ResourceManager.AddTextureSRV("Tonemap", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            quad = new(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/tonemap/vs.hlsl",
                PixelShader = "effects/tonemap/ps.hlsl",
            });
            buffer = device.CreateBuffer(new Params(bloomStrength), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            Output = await ResourceManager.GetTextureRTVAsync("FXAA");
            Bloom = await ResourceManager.GetTextureSRVAsync("Bloom");

            InitUnsafe();
        }

        private unsafe void InitUnsafe()
        {
            srvs = AllocArray(2);
            srvs[0] = (void*)HDR?.NativePointer;
            srvs[1] = (void*)Bloom?.NativePointer;
        }

        public void BeginResize()
        {
            ResourceManager.RequireUpdate("Tonemap");
        }

        public async void EndResize(int width, int height)
        {
            Output = await ResourceManager.GetTextureRTVAsync("FXAA");
            HDR = ResourceManager.UpdateTextureSRV("Tonemap", TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            Bloom = await ResourceManager.GetTextureSRVAsync("Bloom");
            EndResizeUnsafe();
        }

        private unsafe void EndResizeUnsafe()
        {
            srvs[0] = (void*)HDR?.NativePointer;
            srvs[1] = (void*)Bloom?.NativePointer;
        }

        public unsafe void Draw(IGraphicsContext context)
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
            context.ClearState();
        }

        public unsafe void Dispose()
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