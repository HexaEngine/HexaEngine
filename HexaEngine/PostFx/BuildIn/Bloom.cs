namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.UI;
    using HexaEngine.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class Bloom : PostFxBase
    {
        private GraphResourceBuilder creator;

        private IGraphicsPipeline downsample;
        private ConstantBuffer<DownsampleParams> downsampleCBuffer;

        private IGraphicsPipeline upsample;
        private ConstantBuffer<UpsampleParams> upsampleCBuffer;

        private IGraphicsPipeline compose;
        private ConstantBuffer<BloomParams> bloomCBuffer;

        private ISamplerState linearSampler;

        private ResourceRef<Texture2D> bloomTex;
        private Texture2D? lensDirtTex;

        private IRenderTargetView[] mipChainRTVs;
        private IShaderResourceView[] mipChainSRVs;
        private Viewport[] viewports;

        private float filterRadius = 0.003f;
        private float bloomIntensity = 0.04f;
        private float bloomThreshold = 0.9f;
        private float lensDirtIntensity = 1.0f;
        private string lensDirtTexPath = string.Empty;

        private int width;
        private int height;

        public override string Name => "Bloom";

        public override PostFxFlags Flags => PostFxFlags.None;

        public float FilterRadius
        {
            get => filterRadius;
            set => NotifyPropertyChangedAndSet(ref filterRadius, value);
        }

        public float BloomIntensity
        {
            get => bloomIntensity;
            set => NotifyPropertyChangedAndSet(ref bloomIntensity, value);
        }

        public float BloomThreshold
        {
            get => bloomThreshold;
            set => NotifyPropertyChangedAndSet(ref bloomThreshold, value);
        }

        public float LensDirtIntensity
        {
            get => lensDirtIntensity;
            set => NotifyPropertyChangedAndSet(ref lensDirtIntensity, value);
        }

        public string LensDirtTexPath
        {
            get => lensDirtTexPath;
            set => NotifyPropertyChangedAndSetAndReload(ref lensDirtTexPath, value ?? string.Empty);
        }

        #region Structs

        private struct DownsampleParams
        {
            public Vector2 SrcResolution;
            public Vector2 Padd;

            public DownsampleParams(Vector2 srcResolution)
            {
                SrcResolution = srcResolution;
                Padd = default;
            }
        }

        private struct UpsampleParams
        {
            public float FilterRadius;
            public Vector3 Padd;

            public UpsampleParams(float filterRadius)
            {
                FilterRadius = filterRadius;
                Padd = default;
            }
        }

        private struct BloomParams
        {
            public float BloomIntensity;
            public float BloomThreshold;
            public float LensDirtIntensity;
            public float Padd;

            public BloomParams(float bloomIntensity, float bloomThreshold, float lensDirtIntensity)
            {
                BloomIntensity = bloomIntensity;
                BloomThreshold = bloomThreshold;
                LensDirtIntensity = lensDirtIntensity;
                Padd = 0;
            }
        }

        #endregion Structs

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("ColorGrading")
                .RunAfter("HBAO")
                .RunAfter("SSGI")
                .RunAfter("SSR")
                .RunAfter("MotionBlur")
                .RunAfter("AutoExposure")
                .RunAfter("TAA")
                .RunAfter("DepthOfField")
                .RunAfter("ChromaticAberration");

            this.creator = creator;

            downsampleCBuffer = new(device, CpuAccessFlags.Write);
            upsampleCBuffer = new(device, CpuAccessFlags.Write);
            bloomCBuffer = new(device, CpuAccessFlags.Write);

            linearSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            List<ShaderMacro> shaderMacros = new(macros);

            if (!string.IsNullOrEmpty(lensDirtTexPath))
            {
                try
                {
                    lensDirtTex = Texture2D.LoadFromAssets(device, lensDirtTexPath);
                    lensDirtTex.TextureReloaded += TextureReloaded;
                    if (lensDirtTex.Exists)
                    {
                        shaderMacros.Add(new ShaderMacro("LensDirtTex", "1"));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load lens dirt tex", ex.Message);
                    Logger.Log(ex);
                }
            }

            downsample = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/downsample/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            upsample = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/upsample/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            compose = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, shaderMacros.ToArray());

            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];
            viewports = new Viewport[levels];

            for (int i = 0; i < levels; i++)
            {
                var tex = creator.CreateTexture2D($"Bloom{(i == 0 ? "" : $".{i}")}", new(Format.R16G16B16A16Float, currentWidth, currentHeight, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget), false);
                mipChainRTVs[i] = tex.Value.RTV;
                mipChainSRVs[i] = tex.Value.SRV;
                viewports[i] = new(currentWidth, currentHeight);
                currentWidth /= 2;
                currentHeight /= 2;

                if (i == 0)
                {
                    bloomTex = tex;
                }
            }

            this.width = width;
            this.height = height;

            dirty = true;
        }

        private void TextureReloaded(Texture2D obj)
        {
            NotifyReload();
        }

        public override void Resize(int width, int height)
        {
            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];
            viewports = new Viewport[levels];

            for (int i = 0; i < levels; i++)
            {
                var name = $"Bloom{(i == 0 ? "" : $".{i}")}";
                creator.ReleaseResource(name);
                var tex = creator.CreateTexture2D(name, new(Format.R16G16B16A16Float, currentWidth, currentHeight, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget), false).Value;
                mipChainRTVs[i] = tex.RTV;
                mipChainSRVs[i] = tex.SRV;
                viewports[i] = new(currentWidth, currentHeight);
                currentWidth /= 2;
                currentHeight /= 2;
            }

            this.width = width;
            this.height = height;
            dirty = true;
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                context.ClearRenderTargetView(mipChainRTVs[0], default);
                downsampleCBuffer.Update(context, new DownsampleParams(new(width, height)));
                upsampleCBuffer.Update(context, new UpsampleParams(filterRadius));
                bloomCBuffer.Update(context, new(bloomIntensity, bloomThreshold, lensDirtIntensity));
                dirty = false;
            }
        }

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            context.PSSetConstantBuffer(0, downsampleCBuffer);
            context.PSSetSampler(0, linearSampler);
            context.SetGraphicsPipeline(downsample);
            for (int i = 0; i < mipChainRTVs.Length; i++)
            {
                if (i > 0)
                {
                    context.PSSetShaderResource(0, mipChainSRVs[i - 1]);
                }
                else
                {
                    context.PSSetShaderResource(0, Input);
                }

                context.SetRenderTarget(mipChainRTVs[i], null);
                context.SetViewport(viewports[i]);
                context.DrawInstanced(4, 1, 0, 0);
                context.PSSetShaderResource(0, null);
                context.SetRenderTarget(null, null);
            }

            context.PSSetConstantBuffer(0, upsampleCBuffer);
            context.SetGraphicsPipeline(upsample);
            for (int i = mipChainRTVs.Length - 1; i > 0; i--)
            {
                context.SetRenderTarget(mipChainRTVs[i - 1], null);
                context.PSSetShaderResource(0, mipChainSRVs[i]);
                context.SetViewport(viewports[i - 1]);
                context.DrawInstanced(4, 1, 0, 0);
                context.PSSetShaderResource(0, null);
                context.SetRenderTarget(null, null);
            }

            nint* composeSRVs = stackalloc nint[] { Input.NativePointer, bloomTex.Value.SRV.NativePointer, lensDirtTex?.SRV?.NativePointer ?? 0 };
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipeline(compose);
            context.PSSetConstantBuffer(0, bloomCBuffer);
            context.PSSetShaderResources(0, 3, (void**)composeSRVs);
            context.DrawInstanced(4, 1, 0, 0);
            nint* emptySRVs = stackalloc nint[] { 0, 0, 0 };
            context.PSSetShaderResources(0, 3, (void**)emptySRVs);
            context.PSSetConstantBuffer(0, null);
            context.SetGraphicsPipeline(null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            downsample.Dispose();
            upsample.Dispose();
            compose.Dispose();
            downsampleCBuffer.Dispose();
            upsampleCBuffer.Dispose();
            bloomCBuffer.Dispose();
            linearSampler.Dispose();

            lensDirtTex?.Dispose();
            lensDirtTex = null;

            for (int i = 0; i < mipChainRTVs.Length; i++)
            {
                creator.ReleaseResource($"Bloom{(i == 0 ? "" : $".{i}")}");
            }
        }
    }
}