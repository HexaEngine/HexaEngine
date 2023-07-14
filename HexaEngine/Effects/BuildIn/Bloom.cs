#nullable disable

using HexaEngine;

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class Bloom : IPostFx
    {
        private Quad quad;
        private IGraphicsPipeline downsample;
        private IGraphicsPipeline upsample;
        private IBuffer downsampleCB;
        private IBuffer upsampleCB;
        private ISamplerState sampler;

        private IRenderTargetView[] mipChainRTVs;
        private IShaderResourceView[] mipChainSRVs;

        private bool enabled = true;
        private float radius = 0.003f;
        private bool dirty;

        private int width;
        private int height;

        private IShaderResourceView Source;
        private bool disposedValue;
        private int priority = 200;

        public event Action<bool> OnEnabledChanged;

        public event Action<int> OnPriorityChanged;

        public string Name => "Bloom";

        public PostFxFlags Flags => PostFxFlags.NoOutput;

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                dirty = true;
                OnEnabledChanged?.Invoke(value);
            }
        }

        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
                OnPriorityChanged?.Invoke(value);
            }
        }

        public float Radius
        {
            get => radius;
            set
            {
                radius = value; dirty = true;
            }
        }

        #region Structs

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

        #endregion Structs

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)

        {
            disposedValue = false;
            quad = new(device);
            downsampleCB = device.CreateBuffer(new ParamsDownsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            upsampleCB = device.CreateBuffer(new ParamsUpsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            downsample = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/bloom/downsample/vs.hlsl",
                PixelShader = "effects/bloom/downsample/ps.hlsl",
            }, macros);
            upsample = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/bloom/upsample/vs.hlsl",
                PixelShader = "effects/bloom/upsample/ps.hlsl",
            }, macros);

            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainRTVs[i] = ResourceManager2.Shared.AddTexture($"Bloom.{i}", new(Format.R16G16B16A16Float, currentWidth, currentWidth, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget)).Value.RTV;
                mipChainSRVs[i] = ResourceManager2.Shared.GetTexture($"Bloom.{i}").Value.SRV;
                currentWidth /= 2;
                currentHeight /= 2;
            }

            ResourceManager2.Shared.SetOrAddResource("Bloom", ResourceManager2.Shared.GetTexture("Bloom.0").Value);

            this.width = width;
            this.height = height;

            dirty = true;
        }

        public void Resize(int width, int height)
        {
            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainRTVs[i] = ResourceManager2.Shared.UpdateTexture($"Bloom.{i}", new Texture2DDescription(Format.R16G16B16A16Float, currentWidth, currentWidth, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget)).Value.RTV;
                mipChainSRVs[i] = ResourceManager2.Shared.GetTexture($"Bloom.{i}").Value.SRV;
                currentWidth /= 2;
                currentHeight /= 2;
            }

            ResourceManager2.Shared.SetOrAddResource("Bloom", ResourceManager2.Shared.GetTexture("Bloom.0").Value);

            this.width = width;
            this.height = height;
            dirty = true;
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Source = view;
        }

        public void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                context.ClearRenderTargetView(mipChainRTVs[0], default);
                context.Write(downsampleCB, new ParamsDownsample(new(width, height), default));
                context.Write(upsampleCB, new ParamsUpsample(radius, default));
                dirty = false;
            }
        }

        public void Draw(IGraphicsContext context)
        {
            for (int i = 0; i < mipChainRTVs.Length; i++)
            {
                if (i > 0)
                {
                    context.PSSetShaderResource(0, mipChainSRVs[i - 1]);
                }
                else
                {
                    context.PSSetShaderResource(0, Source);
                }

                context.PSSetConstantBuffer(0, downsampleCB);
                context.PSSetSampler(0, sampler);
                context.SetRenderTarget(mipChainRTVs[i], null);
                context.SetViewport(mipChainRTVs[i].Viewport);
                quad.DrawAuto(context, downsample);
                context.ClearState();
            }

            for (int i = mipChainRTVs.Length - 1; i > 0; i--)
            {
                context.SetRenderTarget(mipChainRTVs[i - 1], null);
                context.PSSetShaderResource(0, mipChainSRVs[i]);
                context.PSSetConstantBuffer(0, upsampleCB);
                context.PSSetSampler(0, sampler);
                context.SetViewport(mipChainRTVs[i - 1].Viewport);
                quad.DrawAuto(context, upsample);
            }
            context.ClearState();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
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