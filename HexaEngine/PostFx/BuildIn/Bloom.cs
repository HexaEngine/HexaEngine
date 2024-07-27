﻿namespace HexaEngine.PostFx.BuildIn
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;

    /// <summary>
    /// A post-processing effect for simulating bloom.
    /// </summary>
    [EditorDisplayName("Bloom")]
    public class Bloom : PostFxBase
    {
#nullable disable
        private PostFxGraphResourceBuilder creator;

        private IGraphicsPipelineState downsample;
        private ConstantBuffer<DownsampleParams> downsampleCBuffer;

        private IGraphicsPipelineState upsample;
        private ConstantBuffer<UpsampleParams> upsampleCBuffer;

        private IGraphicsPipelineState compose;
        private ConstantBuffer<BloomParams> bloomCBuffer;

        private ISamplerState linearSampler;

        private ResourceRef<Texture2D> bloomTex;

        private IRenderTargetView[] mipChainRTVs;
        private IShaderResourceView[] mipChainSRVs;
        private Viewport[] viewports;
#nullable restore

        private Texture2D? lensDirtTex;

        private BloomQualityPreset quality = BloomQualityPreset.High;
        private int passes = 8;
        private float filterRadius = 0.003f;
        private float bloomIntensity = 0.04f;
        private float bloomThreshold = 0.0f;
        private float lensDirtIntensity = 1.0f;
        private AssetRef lensDirtAsset;

        private int width;
        private int height;

        /// <inheritdoc/>
        public override string Name => "Bloom";

        /// <inheritdoc/>
        public override PostFxFlags Flags => PostFxFlags.None;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        public enum BloomQualityPreset
        {
            Custom = -1,
            Low = 0,
            Medium = 1,
            High = 2,
            Ultra = 3,
        }

        [EditorProperty<BloomQualityPreset>("Bloom Quality", BloomQualityPreset.High)]
        [Tooltip("Determines the quality of the bloom effect.")]
        public BloomQualityPreset Quality
        {
            get => quality;
            set => quality = value;
        }

        /// <summary>
        /// Gets or sets the number of passes for the bloom effect.
        /// </summary>
        [EditorProperty("Bloom Passes", 8)]
        [Tooltip("(Default: 8) Determines the number of passes for the bloom effect. Only has a effect if quality preset is set to custom, pass count will be clamped to current display size.")]
        public int Passes
        {
            get => passes;
            set
            {
                if (passes == value)
                {
                    return;
                }

                NotifyPropertyChangedAndSet(ref passes, value);
                if (quality == BloomQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter radius for bloom.
        /// </summary>
        [EditorProperty("Filter Radius", 0.003f)]
        [Tooltip("(Default: 0.003) Determines the filter radius for the bloom effect.")]
        public float FilterRadius
        {
            get => filterRadius;
            set => NotifyPropertyChangedAndSet(ref filterRadius, value);
        }

        /// <summary>
        /// Gets or sets the intensity of the bloom effect.
        /// </summary>
        [EditorProperty("Bloom Intensity", 0.04f)]
        [Tooltip("(Default: 0.04) Determines the intensity of the bloom effect.")]
        public float BloomIntensity
        {
            get => bloomIntensity;
            set => NotifyPropertyChangedAndSet(ref bloomIntensity, value);
        }

        /// <summary>
        /// Gets or sets the threshold for triggering the bloom effect.
        /// </summary>
        [EditorProperty("Bloom Threshold", 0.0f)]
        [Tooltip("(Default: 0.0) Determines the threshold for triggering the bloom effect.")]
        public float BloomThreshold
        {
            get => bloomThreshold;
            set => NotifyPropertyChangedAndSet(ref bloomThreshold, value);
        }

        /// <summary>
        /// Gets or sets the intensity of the lens dirt effect.
        /// </summary>
        [EditorProperty("Dirt Mask Intensity", 1.0f)]
        [Tooltip("(Default: 1.0) Determines the intensity of the lens dirt effect.")]
        public float LensDirtIntensity
        {
            get => lensDirtIntensity;
            set => NotifyPropertyChangedAndSet(ref lensDirtIntensity, value);
        }

        /// <summary>
        /// Gets or sets the file path for the lens dirt texture.
        /// </summary>
        [EditorProperty("Dirt Mask Texture", AssetType.Texture2D)]
        [Tooltip("Determines the file path for the lens dirt texture.")]
        public AssetRef DirtMaskAsset
        {
            get => lensDirtAsset;
            set => NotifyPropertyChangedAndSetAndReload(ref lensDirtAsset, value);
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

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                  .RunBefore<ColorGrading>()
                  .RunAfter<HBAO>()
                  .RunAfter<SSGI>()
                  .RunAfter<SSR>()
                  .RunAfter<MotionBlur>()
                  .RunAfter<AutoExposure>()
                  .RunAfter<TAA>()
                  .RunAfter<DepthOfField>()
                  .RunAfter<ChromaticAberration>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;

            downsampleCBuffer = new(CpuAccessFlags.Write);
            upsampleCBuffer = new(CpuAccessFlags.Write);
            bloomCBuffer = new(CpuAccessFlags.Write);

            linearSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            List<ShaderMacro> shaderMacros = new(macros);

            if (lensDirtAsset.Exists())
            {
                try
                {
                    lensDirtTex = Texture2D.LoadFromAssets(lensDirtAsset);
                    shaderMacros.Add(new ShaderMacro("LensDirtTex", "1"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load lens dirt tex", ex.Message);
                    LoggerFactory.GetLogger(nameof(Resources)).Log(ex);
                }
            }

            downsample = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/downsample/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            upsample = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/upsample/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            compose = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/ps.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];
            viewports = new Viewport[levels];

            for (int i = 0; i < levels; i++)
            {
                var tex = creator.CreateTexture2D($"BLOOM_BUFFER_{i}", new(Format.R16G16B16A16Float, currentWidth, currentHeight, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None);
#nullable disable
                mipChainRTVs[i] = tex.Value.RTV;
                mipChainSRVs[i] = tex.Value.SRV;
#nullable enable
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

        /// <inheritdoc/>
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
                var name = $"BLOOM_BUFFER_{i}";
                creator.DisposeResource(name);
                var tex = creator.CreateTexture2D(name, new(Format.R16G16B16A16Float, currentWidth, currentHeight, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None).Value;
#nullable disable
                mipChainRTVs[i] = tex.RTV;
                mipChainSRVs[i] = tex.SRV;
#nullable restore
                viewports[i] = new(currentWidth, currentHeight);
                currentWidth /= 2;
                currentHeight /= 2;
            }

            this.width = width;
            this.height = height;
            dirty = true;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override unsafe void Draw(IGraphicsContext context)
        {
            context.PSSetConstantBuffer(0, downsampleCBuffer);
            context.PSSetSampler(0, linearSampler);
            context.SetPipelineState(downsample);
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
            context.SetPipelineState(upsample);
            for (int i = mipChainRTVs.Length - 1; i > 0; i--)
            {
                context.SetRenderTarget(mipChainRTVs[i - 1], null);
                context.PSSetShaderResource(0, mipChainSRVs[i]);
                context.SetViewport(viewports[i - 1]);
                context.DrawInstanced(4, 1, 0, 0);
                context.PSSetShaderResource(0, null);
                context.SetRenderTarget(null, null);
            }

#nullable disable
            nint* composeSRVs = stackalloc nint[] { Input.NativePointer, bloomTex.Value.SRV.NativePointer, lensDirtTex?.SRV?.NativePointer ?? 0 };
#nullable restore
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetPipelineState(compose);
            context.PSSetConstantBuffer(0, bloomCBuffer);
            context.PSSetShaderResources(0, 3, (void**)composeSRVs);
            context.DrawInstanced(4, 1, 0, 0);
            nint* emptySRVs = stackalloc nint[] { 0, 0, 0 };
            context.PSSetShaderResources(0, 3, (void**)emptySRVs);
            context.PSSetConstantBuffer(0, null);
            context.SetPipelineState(null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
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
                creator.DisposeResource($"BLOOM_BUFFER_{i}");
            }
        }
    }
}