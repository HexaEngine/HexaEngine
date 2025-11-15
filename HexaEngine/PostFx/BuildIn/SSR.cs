namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;

    /// <summary>
    /// Screen Space Reflections (SSR) post-processing effect.
    /// </summary>
    [EditorDisplayName("SSR")]
    public class SSR : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipelineSSR;
        private ConstantBuffer<SSRParams> ssrParamsBuffer;
#nullable restore

        private SSRQualityPreset qualityPreset = SSRQualityPreset.Medium;
        private float intensity = 1;
        private int maxRayCount = 16;
        private int raySteps = 16;
        private float rayStep = 1.60f;
        private float rayHitThreshold = 2.00f;

        /// <inheritdoc/>
        public override string Name { get; } = "SSR";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; }

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        /// <summary>
        /// Enumeration representing the quality presets for SSR.
        /// </summary>
        public enum SSRQualityPreset
        {
            /// <summary>
            /// Custom quality settings.
            /// </summary>
            Custom = -1,

            /// <summary>
            /// Dynamic quality settings.
            /// </summary>
            Dynamic = 0,

            /// <summary>
            /// Low quality settings.
            /// </summary>
            Low = 1,

            /// <summary>
            /// Medium quality settings.
            /// </summary>
            Medium = 2,

            /// <summary>
            /// High quality settings.
            /// </summary>
            High = 3,
        }

        /// <summary>
        /// Structure representing parameters for Screen Space Reflections (SSR).
        /// </summary>
        public struct SSRParams
        {
            /// <summary>
            /// Gets or sets the intensity for SSR.
            /// </summary>
            public float Intensity;

            /// <summary>
            /// Gets or sets the maximum number of rays for SSR.
            /// </summary>
            public int MaxRayCount;

            /// <summary>
            /// Gets or sets the number of steps for SSR ray tracing.
            /// </summary>
            public int RaySteps;

            /// <summary>
            /// Gets or sets the step size for SSR ray tracing.
            /// </summary>
            public float RayStep;

            /// <summary>
            /// Gets or sets the threshold for SSR ray hits.
            /// </summary>
            public float RayHitThreshold;

            private Vector3 padding;

            /// <summary>
            /// Initializes a new instance of the <see cref="SSRParams"/> struct.
            /// </summary>
            /// <param name="maxRayCount">The maximum number of rays for SSR.</param>
            /// <param name="raySteps">The number of steps for SSR ray tracing.</param>
            /// <param name="rayStep">The step size for SSR ray tracing.</param>
            /// <param name="rayHitThreshold">The threshold for SSR ray hits.</param>
            public SSRParams(int maxRayCount, int raySteps, float rayStep, float rayHitThreshold)
            {
                MaxRayCount = maxRayCount;
                RaySteps = raySteps;
                RayStep = rayStep;
                RayHitThreshold = rayHitThreshold;
            }
        }

        /// <summary>
        /// Gets or sets the quality preset for SSR. Set to custom to control max rays and ray steps manually.
        /// </summary>
        [EditorProperty<SSRQualityPreset>("Quality Preset")]
        [Tooltip("Specifies the quality level for Screen Space Reflections (SSR). Higher presets result in more accurate reflections but may impact performance. Set to 'Custom' to manually control maximum rays and ray steps.")]
        public SSRQualityPreset QualityPreset
        {
            get => qualityPreset;
            set
            {
                NotifyPropertyChangedAndSetAndReload(ref qualityPreset, value);
            }
        }

        /// <summary>
        /// Gets or sets the intensity of SSGI.
        /// </summary>
        [EditorProperty("Intensity")]
        [Tooltip("Specifies the intensity of Screen Space Global Illumination (SSGI).")]
        public float Intensity
        {
            get => intensity;
            set => NotifyPropertyChangedAndSet(ref intensity, value);
        }

        /// <summary>
        /// Gets or sets the maximum number of rays for SSR.
        /// </summary>
        [EditorProperty("Max Ray Count")]
        [Tooltip("Specifies the maximum number of rays for Screen Space Reflections (SSR). This value is applicable when the quality preset is set to 'Custom'.")]
        public int MaxRayCount
        {
            get => maxRayCount;
            set
            {
                NotifyPropertyChangedAndSet(ref maxRayCount, value);
                if (qualityPreset == SSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of steps for SSR ray tracing.
        /// </summary>
        [EditorProperty("Ray Steps")]
        [Tooltip("Specifies the number of steps for ray tracing in Screen Space Reflections (SSR). This value is applicable when the quality preset is set to 'Custom'.")]
        public int RaySteps
        {
            get => raySteps;
            set
            {
                NotifyPropertyChangedAndSet(ref raySteps, value);
                if (qualityPreset == SSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the step size for SSR ray tracing.
        /// </summary>
        [EditorProperty("Ray Step")]
        [Tooltip("Specifies the step size for ray tracing in Screen Space Reflections (SSR). This value is applicable when the quality preset is set to 'Custom'.")]
        public float RayStep
        {
            get => rayStep;
            set
            {
                NotifyPropertyChangedAndSet(ref rayStep, value);
                if (qualityPreset == SSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the threshold for SSR ray hits.
        /// </summary>
        [EditorProperty("Ray Hit Threshold")]
        [Tooltip("Specifies the threshold for ray hits in Screen Space Reflections (SSR). This value is applicable when the quality preset is set to 'Custom'.")]
        public float RayHitThreshold
        {
            get => rayHitThreshold;
            set
            {
                NotifyPropertyChangedAndSet(ref rayHitThreshold, value);
                if (qualityPreset == SSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<LinearDepth>()
                .RunBefore<ColorGrading>()
                .RunAfter<HBAO>()
                .RunAfter<SSGI>()
                .RunBefore<MotionBlur>()
                .RunBefore<AutoExposure>()
                .RunBefore<TAA>()
                .RunBefore<DepthOfField>()
                .RunBefore<ChromaticAberration>()
                .RunBefore<Bloom>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("SSR_QUALITY", ((int)qualityPreset).ToString())
            };
            if (qualityPreset == SSRQualityPreset.Custom)
            {
                shaderMacros.Add(new("SSR_MAX_RAY_COUNT", maxRayCount));
                shaderMacros.Add(new("SSR_RAY_STEPS", raySteps));
                shaderMacros.Add(new("SSR_RAY_STEP", rayStep));
                shaderMacros.Add(new("SSR_RAY_HIT_THRESHOLD", rayHitThreshold));
            }

            SSRParams ssrParams = default;
            ssrParams.Intensity = intensity;
            ssrParams.MaxRayCount = maxRayCount;
            ssrParams.RaySteps = raySteps;
            ssrParams.RayStep = rayStep;
            ssrParams.RayHitThreshold = rayHitThreshold;
            ssrParamsBuffer = new(ssrParams, CpuAccessFlags.Write);

            pipelineSSR = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
                PixelShader = "HexaEngine.PostFx:shaders/effects/ssr/ps.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        public override void UpdateBindings()
        {
            pipelineSSR.Bindings.SetSRV("inputTex", Input);
            pipelineSSR.Bindings.SetCBV("SSRParams", ssrParamsBuffer);
        }

        /// <inheritdoc/>
        public override unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            if (ssrParamsBuffer != null)
            {
                SSRParams ssrParams = default;
                ssrParams.Intensity = intensity;
                ssrParams.MaxRayCount = maxRayCount;
                ssrParams.RaySteps = raySteps;
                ssrParams.RayStep = rayStep;
                ssrParams.RayHitThreshold = rayHitThreshold;
                ssrParamsBuffer.Update(context, ssrParams);
            }

            context.SetGraphicsPipelineState(pipelineSSR);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipelineSSR.Dispose();
            ssrParamsBuffer?.Dispose();
            ssrParamsBuffer = null;
        }
    }
}