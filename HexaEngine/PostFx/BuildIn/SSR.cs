namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.PostFx;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Screen Space Reflections (SSR) post-processing effect.
    /// </summary>
    [EditorDisplayName("SSR")]
    public class SSR : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState blendPSO;
        private IComputePipelineState computePSO;
        private ConstantBuffer<SSRParams> ssrParamsBuffer;
        private ResourceRef<Texture2D> temporalBuffer0;
        private ResourceRef<Texture2D> temporalBuffer1;
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
            /// Represents the dimensions as a two-dimensional vector.
            /// </summary>
            public Vector2 Dims;

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

            private float padding;

            /// <summary>
            /// Initializes a new instance of the <see cref="SSRParams"/> struct.
            /// </summary>
            /// <param name="maxRayCount">The maximum number of rays for SSR.</param>
            /// <param name="dims"></param>
            /// <param name="raySteps">The number of steps for SSR ray tracing.</param>
            /// <param name="rayStep">The step size for SSR ray tracing.</param>
            /// <param name="rayHitThreshold">The threshold for SSR ray hits.</param>
            public SSRParams(int maxRayCount, Vector2 dims, int raySteps, float rayStep, float rayHitThreshold)
            {
                MaxRayCount = maxRayCount;
                Dims = dims;
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
                .RunBefore<Bloom>()
                .AddBinding("VelocityBuffer");
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
            shaderMacros.Add(new("SSR_TEMPORAL", "1"));

            SSRParams ssrParams = default;
            ssrParams.Intensity = intensity;
            ssrParams.Dims = new(width, height);
            ssrParams.MaxRayCount = maxRayCount;
            ssrParams.RaySteps = raySteps;
            ssrParams.RayStep = rayStep;
            ssrParams.RayHitThreshold = rayHitThreshold;
            ssrParamsBuffer = new(ssrParams, CpuAccessFlags.Write);

            computePSO = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "HexaEngine.PostFx:shaders/effects/ssr/cs.hlsl",
                Macros = [.. shaderMacros]
            });

            blendPSO = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
                PixelShader = "HexaEngine.PostFx:shaders/effects/ssr/blend.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            temporalBuffer0 = creator.CreateBuffer("SSR_TemporalBuffer0", creationFlags: ResourceCreationFlags.None);
            temporalBuffer1 = creator.CreateBuffer("SSR_TemporalBuffer1");
        }

        public override void UpdateBindings()
        {
            computePSO.Bindings.SetSRV("inputTex", Input);
            computePSO.Bindings.SetCBV("SSRParams", ssrParamsBuffer);
            computePSO.Bindings.SetSRV("temporalBuffer", temporalBuffer0.Value);
            computePSO.Bindings.SetUAV("outputTex", temporalBuffer1.Value);

            blendPSO.Bindings.SetSRV("inputTex", Input);
            blendPSO.Bindings.SetSRV("indirectTex", temporalBuffer0.Value);
        }

        public override void Update(IGraphicsContext context)
        {
            if (ssrParamsBuffer != null)
            {
                var viewport = temporalBuffer0.Value!.Viewport;
                SSRParams ssrParams = default;
                ssrParams.Intensity = intensity;
                ssrParams.Dims = new(viewport.Width, viewport.Height);
                ssrParams.MaxRayCount = maxRayCount;
                ssrParams.RaySteps = raySteps;
                ssrParams.RayStep = rayStep;
                ssrParams.RayHitThreshold = rayHitThreshold;
                ssrParamsBuffer.Update(context, ssrParams);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CeilDiv(uint x, uint y)
        {
            return (x + y - 1) / y;
        }

        /// <inheritdoc/>
        public override unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            var viewport = temporalBuffer0.Value!.Viewport;
            context.SetComputePipelineState(computePSO);
            context.Dispatch(CeilDiv((uint)viewport.Width, 32), CeilDiv((uint)viewport.Height, 32), 1);
            context.SetComputePipelineState(null);

            context.CopyResource(temporalBuffer0.Value!, temporalBuffer1.Value!);

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(blendPSO);
            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            blendPSO?.Dispose();
            blendPSO = null;
            computePSO?.Dispose();
            computePSO = null;
            ssrParamsBuffer?.Dispose();
            ssrParamsBuffer = null;
        }
    }
}