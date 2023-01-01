namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;

    public class Bloom : IEffect
    {
        private readonly Quad quad;
        private readonly GraphicsPipeline downsample;
        private readonly GraphicsPipeline upsample;
        private readonly IBuffer downsampleCB;
        private readonly IBuffer upsampleCB;
        private readonly ISamplerState sampler;

        private ITexture2D[] mipChainTex;
        private IRenderTargetView[] mipChainRTVs;
        private IShaderResourceView[] mipChainSRVs;

        private bool enabled;
        private float radius = 0.003f;
        private bool dirty;

        private int width;
        private int height;

        public IShaderResourceView? Source;
        private bool disposedValue;

        public IShaderResourceView Output => mipChainSRVs[0];

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

        public Bloom(IGraphicsDevice device)
        {
            quad = new(device);
            mipChainTex = Array.Empty<ITexture2D>();
            mipChainRTVs = Array.Empty<IRenderTargetView>();
            mipChainSRVs = Array.Empty<IShaderResourceView>();
            downsampleCB = device.CreateBuffer(new ParamsDownsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            upsampleCB = device.CreateBuffer(new ParamsUpsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            downsample = new(device, new()
            {
                VertexShader = "effects/bloom/downsample/vs.hlsl",
                PixelShader = "effects/bloom/downsample/ps.hlsl",
            });
            upsample = new(device, new()
            {
                VertexShader = "effects/bloom/upsample/vs.hlsl",
                PixelShader = "effects/bloom/upsample/ps.hlsl",
            });
            dirty = true;
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

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = MathUtil.ComputeMipLevels(currentWidth, currentHeight);

            for (int i = 0; i < mipChainTex?.Length; i++)
            {
                mipChainTex[i]?.Dispose();
                mipChainRTVs[i]?.Dispose();
                mipChainSRVs[i]?.Dispose();
            }

            mipChainTex = new ITexture2D[levels];
            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainTex[i] = device.CreateTexture2D(Format.RGBA32Float, currentWidth, currentHeight, 1, 1, null, BindFlags.RenderTarget | BindFlags.ShaderResource);
                mipChainRTVs[i] = device.CreateRenderTargetView(mipChainTex[i], new(0, 0, currentWidth, currentHeight));
                mipChainSRVs[i] = device.CreateShaderResourceView(mipChainTex[i]);
                currentWidth /= 2;
                currentHeight /= 2;
            }
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
                for (int i = 0; i < mipChainTex?.Length; i++)
                {
                    mipChainTex[i]?.Dispose();
                    mipChainRTVs[i]?.Dispose();
                    mipChainSRVs[i]?.Dispose();
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