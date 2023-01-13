#nullable disable

namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Resources;
    using System.Numerics;

    public class Bloom : IEffect
    {
        private Quad quad;
        private IGraphicsPipeline downsample;
        private IGraphicsPipeline upsample;
        private IBuffer downsampleCB;
        private IBuffer upsampleCB;
        private ISamplerState sampler;

        private IRenderTargetView[] mipChainRTVs;
        private IShaderResourceView[] mipChainSRVs;

        private bool enabled;
        private float radius = 0.003f;
        private bool dirty;

        private int width;
        private int height;

        private IShaderResourceView Source;
        private bool disposedValue;

        private struct ParamsDownsample
        {
            public Vector2 SrcResolution;
            public Vector2 Padd;

            public ParamsDownsample(Vector2 srcResolution, Vector2 padd)
            {
                SrcResolution = srcResolution;
                Padd = padd;
            }
        }

        private struct ParamsUpsample
        {
            public float FilterRadius;
            public Vector3 Padd;

            public ParamsUpsample(float filterRadius, Vector3 padd)
            {
                FilterRadius = filterRadius;
                Padd = padd;
            }
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                dirty = true;
            }
        }

        public float Radius
        { get => radius; set { radius = value; dirty = true; } }

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            quad = new(device);
            downsampleCB = device.CreateBuffer(new ParamsDownsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            upsampleCB = device.CreateBuffer(new ParamsUpsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            downsample = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/bloom/downsample/vs.hlsl",
                PixelShader = "effects/bloom/downsample/ps.hlsl",
            });
            upsample = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/bloom/upsample/vs.hlsl",
                PixelShader = "effects/bloom/upsample/ps.hlsl",
            });

            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = MathUtil.ComputeMipLevels(currentWidth, currentHeight);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainRTVs[i] = ResourceManager.AddTextureRTV($"Bloom.{i}", TextureDescription.CreateTexture2DWithRTV(currentWidth, currentHeight, 1)) ?? throw new InvalidOperationException();
                mipChainSRVs[i] = ResourceManager.GetTextureSRV($"Bloom.{i}") ?? throw new InvalidOperationException();
                currentWidth /= 2;
                currentHeight /= 2;
            }

            ResourceManager.SetOrAddResource("Bloom", ResourceManager.GetTexture("Bloom.0") ?? throw new InvalidOperationException());

            this.width = width;
            this.height = height;

            dirty = true;

            Source = await ResourceManager.GetTextureSRVAsync("Tonemap");
        }

        public void BeginResize()
        {
            ResourceManager.RequireUpdate("Bloom");
        }

        public async void EndResize(int width, int height)
        {
            Source = await ResourceManager.GetTextureSRVAsync("Tonemap");

            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = MathUtil.ComputeMipLevels(currentWidth, currentHeight);

            for (int i = 0; i < mipChainRTVs?.Length; i++)
            {
                ResourceManager.RemoveResource($"Bloom.{i}");
            }

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainRTVs[i] = ResourceManager.AddTextureRTV($"Bloom.{i}", TextureDescription.CreateTexture2DWithRTV(currentWidth, currentHeight, 1)) ?? throw new InvalidOperationException();
                mipChainSRVs[i] = ResourceManager.GetTextureSRV($"Bloom.{i}") ?? throw new InvalidOperationException();
                currentWidth /= 2;
                currentHeight /= 2;
            }

            ResourceManager.SetOrAddResource("Bloom", ResourceManager.GetTexture("Bloom.0") ?? throw new InvalidOperationException());

            this.width = width;
            this.height = height;
            dirty = true;
        }

        public void Draw(IGraphicsContext context)
        {
            if (dirty)
            {
                context.ClearRenderTargetView(mipChainRTVs[0], default);
                context.Write(downsampleCB, new ParamsDownsample(new(width, height), default));
                context.Write(upsampleCB, new ParamsUpsample(radius, default));
                dirty = false;
            }

            if (!enabled)
            {
                return;
            }

            for (int i = 0; i < mipChainRTVs.Length; i++)
            {
                if (i > 0)
                {
                    context.PSSetShaderResource(mipChainSRVs[i - 1], 0);
                }
                else
                {
                    context.PSSetShaderResource(Source, 0);
                }

                context.PSSetConstantBuffer(downsampleCB, 0);
                context.PSSetSampler(sampler, 0);
                context.SetRenderTarget(mipChainRTVs[i], null);
                quad.DrawAuto(context, downsample, mipChainRTVs[i].Viewport);
                context.ClearState();
            }

            for (int i = mipChainRTVs.Length - 1; i > 0; i--)
            {
                context.SetRenderTarget(mipChainRTVs[i - 1], null);
                context.PSSetShaderResource(mipChainSRVs[i], 0);
                context.PSSetConstantBuffer(upsampleCB, 0);
                context.PSSetSampler(sampler, 0);
                quad.DrawAuto(context, upsample, mipChainRTVs[i - 1].Viewport);
            }
            context.ClearState();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
                for (int i = 0; i < mipChainRTVs?.Length; i++)
                {
                    ResourceManager.RemoveResource($"Bloom.{i}");
                }
                downsample.Dispose();
                upsample.Dispose();
                downsampleCB.Dispose();
                upsampleCB.Dispose();
                sampler.Dispose();
                disposedValue = true;
            }
        }

        ~Bloom()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}