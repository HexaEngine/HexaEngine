namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.PostFx;
    using HexaEngine.UI;
    using System.Numerics;

    /// <summary>
    /// Screen Space Reflections (SSR) post-processing effect.
    /// </summary>
    [EditorDisplayName("DDA SSR")]
    public class DDASSR : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipelineSSR;
        private ConstantBuffer<DDASSRParams> ssrParamsBuffer;
#nullable restore

        private DDASSRQualityPreset qualityPreset = DDASSRQualityPreset.Medium;
        private float intensity = 1;
        private float zThickness = 0.05f;
        private float stride = 10;
        private int maxSteps = 100;
        private float maxDistance = 10;
        private float strideZCutoff = 100;

        /// <inheritdoc/>
        public override string Name { get; } = "DDA SSR";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; }

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        /// <summary>
        /// Enumeration representing the quality presets for SSR.
        /// </summary>
        public enum DDASSRQualityPreset
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
        public struct DDASSRParams
        {
            public float Intensity;
            public float ZThickness;
            public float Stride;
            public int MaxSteps;
            public float MaxDistance;
            public float StrideZCutoff;

            private Vector2 padding;
        }

        /// <summary>
        /// Gets or sets the quality preset for SSR. Set to custom to control max rays and ray steps manually.
        /// </summary>
        [EditorProperty<DDASSRQualityPreset>("Quality Preset")]
        [Tooltip("Specifies the quality level for Screen Space Reflections (SSR). Higher presets result in more accurate reflections but may impact performance. Set to 'Custom' to manually control maximum rays and ray steps.")]
        public DDASSRQualityPreset QualityPreset
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
        /// Gets or sets the thickness to ascribe to each pixel in the depth buffer.
        /// </summary>
        [EditorProperty("Z Thickness")]
        [Tooltip("Specifies the thickness to ascribe to each pixel in the depth buffer.")]
        public float ZThickness
        {
            get => zThickness;
            set
            {
                NotifyPropertyChangedAndSet(ref zThickness, value);
                if (qualityPreset == DDASSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Step in horizontal or vertical pixels between samples.
        /// </summary>
        [EditorProperty("Stride")]
        [Tooltip("Specifies the Step in horizontal or vertical pixels between samples.")]
        public float Stride
        {
            get => stride;
            set
            {
                NotifyPropertyChangedAndSet(ref stride, value);
                if (qualityPreset == DDASSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of steps for SSR ray tracing.
        /// </summary>
        [EditorProperty("Max Steps")]
        [Tooltip("Specifies the number of steps for ray tracing in Screen Space Reflections (SSR). This value is applicable when the quality preset is set to 'Custom'.")]
        public int MaxSteps
        {
            get => maxSteps;
            set
            {
                NotifyPropertyChangedAndSet(ref maxSteps, value);
                if (qualityPreset == DDASSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum camera-space distance to trace before returning a miss.
        /// </summary>
        [EditorProperty("Max Distance")]
        [Tooltip("Specifies the maximum camera-space distance to trace before returning a miss.")]
        public float MaxDistance
        {
            get => maxDistance;
            set
            {
                NotifyPropertyChangedAndSet(ref maxDistance, value);
                if (qualityPreset == DDASSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the more distant pixels are smaller in screen space. This value tells at what point to.
        /// </summary>
        [EditorProperty("Stride Z Cutoff")]
        [Tooltip("the more distant pixels are smaller in screen space. This value tells at what point to.")]
        public float StrideZCutoff
        {
            get => strideZCutoff;
            set
            {
                NotifyPropertyChangedAndSet(ref strideZCutoff, value);
                if (qualityPreset == DDASSRQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunAfter<HBAO>()
                .RunAfter<SSGI>()
                .RunBefore<MotionBlur>()
                .RunBefore<AutoExposure>()
                .RunBefore<TAA>()
                .RunBefore<DepthOfField>()
                .RunBefore<ChromaticAberration>()
                .RunBefore<Bloom>()
                .Override<SSR>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("SSR_QUALITY", ((int)qualityPreset).ToString())
            };
            if (qualityPreset == DDASSRQualityPreset.Custom)
            {
            }

            DDASSRParams ssrParams = default;
            ssrParams.Intensity = intensity;
            ssrParams.ZThickness = zThickness;
            ssrParams.Stride = stride;
            ssrParams.MaxSteps = maxSteps;
            ssrParams.MaxDistance = maxDistance;
            ssrParams.StrideZCutoff = strideZCutoff;
            ssrParamsBuffer = new(ssrParams, CpuAccessFlags.Write);

            pipelineSSR = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
                PixelShader = "HexaEngine.PostFx:shaders/effects/ssr/dda.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        public override void UpdateBindings()
        {
            pipelineSSR.Bindings.SetSRV("inputTex", Input);
            pipelineSSR.Bindings.SetCBV("DDASSRParams", ssrParamsBuffer);
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
                DDASSRParams ssrParams = default;
                ssrParams.Intensity = intensity;
                ssrParams.ZThickness = zThickness;
                ssrParams.Stride = stride;
                ssrParams.MaxSteps = maxSteps;
                ssrParams.MaxDistance = maxDistance;
                ssrParams.StrideZCutoff = strideZCutoff;
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