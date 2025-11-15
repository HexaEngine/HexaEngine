#nullable disable

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System;
    using System.Numerics;

    /// <summary>
    /// An auto-exposure post-processing effect.
    /// </summary>
    [EditorDisplayName("Auto Exposure")]
    public class AutoExposure : PostFxBase
    {
        private PostFxGraphResourceBuilder creator;
        private int width;
        private int height;

        private IComputePipelineState lumaCompute;
        private ConstantBuffer<LumaParams> lumaParams;
        private UavBuffer<uint> histogram;

        private IComputePipelineState lumaAvgCompute;
        private ConstantBuffer<LumaAvgParams> lumaAvgParams;
        private ResourceRef<Texture2D> lumaTex;

        private IGraphicsPipelineState compose;

        private float minLogLuminance = -8;
        private float maxLogLuminance = 3;
        private float tau = 1.1f;

        /// <inheritdoc/>
        public override string Name => "AutoExposure";

        /// <inheritdoc/>
        public override PostFxFlags Flags => PostFxFlags.None;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        /// <summary>
        /// Gets or sets the minimum log luminance value.
        /// </summary>
        [EditorProperty("Min Log Luminance", -8)]
        [Tooltip("(Default: -8) Minimum log luminance determines the lowest logarithmic value of light intensity considered by auto exposure. Pixels with a log luminance value below this threshold are considered too dark and may be adjusted for correct exposure. Lower values include more dark pixels in exposure calculations, resulting in an overall brighter scene. Be cautious as setting this too low may also amplify noise or artifacts.")]
        public unsafe float MinLogLuminance
        {
            get => minLogLuminance;
            set => NotifyPropertyChangedAndSet(ref minLogLuminance, value);
        }

        /// <summary>
        /// Gets or sets the maximum log luminance value.
        /// </summary>
        [EditorProperty("Max Log Luminance", 3)]
        [Tooltip("(Default: 3) Maximum log luminance determines the highest logarithmic value of light intensity considered by auto exposure. Pixels with a log luminance value above this threshold are considered too bright and may be adjusted for correct exposure. A higher value limits the brightness of the scene to avoid overexposure.")]
        public unsafe float MaxLogLuminance
        {
            get => maxLogLuminance;
            set => NotifyPropertyChangedAndSet(ref maxLogLuminance, value);
        }

        /// <summary>
        /// Gets or sets the tau value.
        /// </summary>
        [EditorProperty("Tau", 1.1f)]
        [Tooltip("(Default: 1.1) Tau determines the rate at which exposure adjusts to changes in luminance. A higher Tau value results in smoother and slower exposure adjustments, reducing rapid fluctuations in lighting conditions. Conversely, a lower Tau value leads to faster exposure adjustments, allowing the camera to react more quickly to changes in brightness.")]
        public unsafe float Tau
        {
            get => tau;
            set => NotifyPropertyChangedAndSet(ref tau, value);
        }

        #region Structs

        private struct LumaParams
        {
            public uint InputWidth;
            public uint InputHeight;
            public float MinLogLuminance;
            public float OneOverLogLuminanceRange;

            public LumaParams(uint width, uint height)
            {
                InputWidth = width;
                InputHeight = height;
                MinLogLuminance = -8.0f;
                OneOverLogLuminanceRange = 1.0f / (3.0f + 8.0f);
            }

            public LumaParams(uint inputWidth, uint inputHeight, float minLogLuminance, float oneOverLogLuminanceRange) : this(inputWidth, inputHeight)
            {
                MinLogLuminance = minLogLuminance;
                OneOverLogLuminanceRange = oneOverLogLuminanceRange;
            }
        }

        private struct LumaAvgParams
        {
            public uint PixelCount;
            public float MinLogLuminance;
            public float LogLuminanceRange;
            public float TimeDelta;
            public float Tau;
            public Vector3 Padd;

            public LumaAvgParams(uint pixelCount)
            {
                PixelCount = pixelCount;
                MinLogLuminance = -8.0f;
                LogLuminanceRange = 3.0f + 8.0f;
                Tau = 1.1f;
                Padd = default;
            }

            public LumaAvgParams(uint pixelCount, float minLogLuminance, float logLuminanceRange, float timeDelta, float tau, Vector3 padd) : this(pixelCount)
            {
                MinLogLuminance = minLogLuminance;
                LogLuminanceRange = logLuminanceRange;
                TimeDelta = timeDelta;
                Tau = tau;
                Padd = padd;
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
             .RunBefore<TAA>()
             .RunBefore<DepthOfField>()
             .RunBefore<ChromaticAberration>()
             .RunBefore<Bloom>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;
            this.width = width;
            this.height = height;

            LumaAvgParams lumaAvg;
            lumaAvg.PixelCount = (uint)(width * height);
            lumaAvg.MinLogLuminance = minLogLuminance;
            lumaAvg.LogLuminanceRange = Math.Abs(maxLogLuminance) + Math.Abs(minLogLuminance);
            lumaAvg.TimeDelta = Time.Delta;
            lumaAvg.Tau = tau;
            lumaAvg.Padd = default;

            lumaAvgParams = new(lumaAvg, CpuAccessFlags.Write);

            LumaParams luma;
            luma.MinLogLuminance = minLogLuminance;
            luma.OneOverLogLuminanceRange = 1.0f / lumaAvg.LogLuminanceRange;
            luma.InputWidth = (uint)width;
            luma.InputHeight = (uint)height;

            lumaParams = new(luma, CpuAccessFlags.Write);

            lumaCompute = device.CreateComputePipelineState(new ComputePipelineDesc("HexaEngine.PostFx:shaders/compute/luma/shader.hlsl"));
            lumaAvgCompute = device.CreateComputePipelineState(new ComputePipelineDesc("HexaEngine.PostFx:shaders/compute/lumaAvg/shader.hlsl"));
            compose = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                PixelShader = "HexaEngine.PostFx:shaders/effects/autoexposure/ps.hlsl",
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            histogram = new(device, 256, CpuAccessFlags.None, Format.R32Typeless, BufferUnorderedAccessViewFlags.Raw);

            lumaTex = creator.CreateTexture2D("Luma", new Texture2DDescription(Format.R32Float, 1, 1, 1, 1, GpuAccessFlags.UA | GpuAccessFlags.Read), ResourceCreationFlags.None);
        }

        public override void UpdateBindings()
        {
            lumaCompute.Bindings.SetCBV("LuminanceHistogramBuffer", lumaParams);
            lumaCompute.Bindings.SetSRV("HDRTexture", Input);
            lumaCompute.Bindings.SetUAV("LuminanceHistogram", histogram.UAV);

            lumaAvgCompute.Bindings.SetCBV("LuminanceHistogramAverageBuffer", lumaAvgParams);
            lumaAvgCompute.Bindings.SetUAV("LuminanceHistogram", histogram.UAV);
            lumaAvgCompute.Bindings.SetUAV("LuminanceOutput", lumaTex.Value.UAV);

            compose.Bindings.SetSRV("hdrTexture", Input);
            compose.Bindings.SetSRV("lumaTexture", lumaTex.Value.SRV);
        }

        /// <inheritdoc/>
        public override unsafe void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            dirty = true;
        }

        /// <inheritdoc/>
        public override unsafe void Update(IGraphicsContext context)
        {
            LumaAvgParams lumaAvg;
            lumaAvg.PixelCount = (uint)(width * height);
            lumaAvg.MinLogLuminance = minLogLuminance;
            lumaAvg.LogLuminanceRange = Math.Abs(maxLogLuminance) + Math.Abs(minLogLuminance);
            lumaAvg.TimeDelta = Time.Delta;
            lumaAvg.Tau = tau;
            lumaAvg.Padd = default;
            lumaAvgParams.Update(context, lumaAvg);

            if (dirty)
            {
                LumaParams luma;
                luma.MinLogLuminance = minLogLuminance;
                luma.OneOverLogLuminanceRange = 1.0f / lumaAvg.LogLuminanceRange;
                luma.InputWidth = (uint)width;
                luma.InputHeight = (uint)height;
                lumaParams.Update(context, luma);
                dirty = false;
            }
        }

        /// <inheritdoc/>
        public override unsafe void Draw(IGraphicsContext context)
        {
            context.SetComputePipelineState(lumaCompute);
            context.Dispatch((uint)width / 16, (uint)height / 16, 1);
            context.SetComputePipelineState(null);

            context.SetComputePipelineState(lumaAvgCompute);
            context.Dispatch(1, 1, 1);
            context.SetComputePipelineState(null);

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(compose);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);

            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            lumaCompute.Dispose();
            lumaParams.Dispose();
            histogram.Dispose();

            lumaAvgCompute.Dispose();
            lumaAvgParams.Dispose();
            compose.Dispose();
            creator.DisposeResource("Luma");
        }
    }
}