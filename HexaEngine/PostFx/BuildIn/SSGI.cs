namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    [EditorDisplayName("SSGI")]
    public class SSGI : PostFxBase
    {
#nullable disable
        private PostFxGraphResourceBuilder creator;
        private IGraphicsPipelineState psoSSGI;
        private IGraphicsPipelineState psoDenoise;
        private IGraphicsPipelineState psoBlend;

        private ConstantBuffer<SSGIParams> ssgiParamsBuffer;
        private ConstantBuffer<DenoiseParams> denoiseParamsBuffer;
        private ResourceRef<Texture2D> ssgiBuffer;
        private Texture2D tempSsgiBuffer;
        private GaussianBlur blur;

#nullable restore

        private float intensity = 1;
        private float distance = 10;
        private int maxRayCount = 16;
        private int raySteps = 8;
        private float rayStep = 0.1f;
        private float rayHitThreshold = 2f;
        private SSGIQualityPreset qualityPreset;
        private float sigmaS = 2.0f;
        private float sigmaR = 0.1f;
        private float radius = 16.0f;

        /// <inheritdoc/>
        public override string Name { get; } = "SSGI";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; } = PostFxFlags.Inline;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        /// <summary>
        /// Specifies the quality level for Screen Space Global Illumination (SSGI).
        /// </summary>
        public enum SSGIQualityPreset
        {
            /// <summary>
            /// Custom quality level.
            /// </summary>
            Custom = -1,

            /// <summary>
            /// Dynamic quality level.
            /// </summary>
            Dynamic = 0,

            /// <summary>
            /// Low quality level.
            /// </summary>
            Low = 1,

            /// <summary>
            /// Medium quality level.
            /// </summary>
            Medium = 2,

            /// <summary>
            /// High quality level.
            /// </summary>
            High = 3,

            /// <summary>
            /// Extreme quality level.
            /// </summary>
            Extreme = 4,
        }

        /// <summary>
        /// Gets or sets the quality preset for SSGI.
        /// </summary>
        [EditorProperty<SSGIQualityPreset>("Quality Preset")]
        [Tooltip("Gets or sets the quality preset for Screen Space Global Illumination (SSGI).")]
        public SSGIQualityPreset QualityPreset
        {
            get => qualityPreset;
            set => NotifyPropertyChangedAndSetAndReload(ref qualityPreset, value);
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
        /// Gets or sets the distance for SSGI.
        /// </summary>
        [EditorProperty("Distance")]
        [Tooltip("Specifies the distance for Screen Space Global Illumination (SSGI).")]
        public float Distance
        {
            get => distance;
            set => NotifyPropertyChangedAndSet(ref distance, value);
        }

        /// <summary>
        /// Gets or sets the maximum number of rays for SSGI.
        /// </summary>
        [EditorProperty("Max Ray Count")]
        [Tooltip("Specifies the maximum number of rays for Screen Space Global Illumination (SSGI). This value is applicable when the quality preset is set to 'Custom'.")]
        public int MaxRayCount
        {
            get => maxRayCount;
            set
            {
                NotifyPropertyChangedAndSet(ref maxRayCount, value);
                if (qualityPreset == SSGIQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of steps for SSGI ray tracing.
        /// </summary>
        [EditorProperty("Ray Steps")]
        [Tooltip("Specifies the number of steps for ray tracing in Screen Space Global Illumination (SSGI). This value is applicable when the quality preset is set to 'Custom'.")]
        public int RaySteps
        {
            get => raySteps;
            set
            {
                NotifyPropertyChangedAndSet(ref raySteps, value);
                if (qualityPreset == SSGIQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ray step for SSGI.
        /// </summary>
        [EditorProperty("Ray Step")]
        [Tooltip("Specifies the ray step for Screen Space Global Illumination (SSGI). This value is applicable when the quality preset is set to 'Custom'.")]
        public float RayStep
        {
            get => rayStep;
            set
            {
                NotifyPropertyChangedAndSet(ref rayStep, value);
                if (qualityPreset == SSGIQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ray step for SSGI.
        /// </summary>
        [EditorProperty("Ray Hit Threshold")]
        [Tooltip("Specifies the ray hit threshold for Screen Space Global Illumination (SSGI). This value is applicable when the quality preset is set to 'Custom'.")]
        public float RayHitThreshold
        {
            get => rayHitThreshold;
            set
            {
                NotifyPropertyChangedAndSet(ref rayHitThreshold, value);
                if (qualityPreset == SSGIQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        [EditorProperty("Denoise Sigma S")]
        public float SigmaS { get => sigmaS; set => NotifyPropertyChangedAndSet(ref sigmaS, value); }

        [EditorProperty("Denoise Sigma R")]
        public float SigmaR { get => sigmaR; set => NotifyPropertyChangedAndSet(ref sigmaR, value); }

        [EditorProperty("Denoise Radius")]
        public float Radius { get => radius; set => NotifyPropertyChangedAndSet(ref radius, value); }

        #region Structs

        private struct SSGIParams
        {
            public float Intensity;
            public float Distance;
            public int MaxRayCount;
            public int RaySteps;
            public float RayStep;
            public float RayHitThreshold;
            public Vector2 Padding;

            public SSGIParams()
            {
                Intensity = 1f;
                Distance = 10;
            }
        }

        private struct DenoiseParams
        {
            public float SigmaS;
            public float SigmaR;
            public float Radius;
            public float Padding;

            public DenoiseParams(float sigmaS, float sigmaR, float radius)
            {
                SigmaS = sigmaS;
                SigmaR = sigmaR;
                Radius = radius;
            }
        }

        #endregion Structs

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                   .RunBefore<ColorGrading>()
                   .RunAfter<HBAO>()
                   .RunBefore<SSR>()
                   .RunBefore<MotionBlur>()
                   .RunBefore<AutoExposure>()
                   .RunBefore<TAA>()
                   .RunBefore<DepthOfField>()
                   .RunBefore<ChromaticAberration>()
                   .RunBefore<Bloom>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;

            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("SSGI_QUALITY", ((int)qualityPreset).ToString())
            };
            if (qualityPreset == SSGIQualityPreset.Custom)
            {
                shaderMacros.Add(new("SSGI_RAY_COUNT", maxRayCount));
                shaderMacros.Add(new("SSGI_RAY_STEPS", raySteps));
                shaderMacros.Add(new("SSGI_RAY_STEP", rayStep));
                shaderMacros.Add(new("SSGI_RAY_HIT_THRESHOLD", rayHitThreshold));
            }

            psoSSGI = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssgi/ps2.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultAdditiveFullscreen);

            psoDenoise = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssgi/denoise.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            psoBlend = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssgi/blend.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            Unsafe.SkipInit<SSGIParams>(out var ssgiParams);
            ssgiParams.Intensity = intensity;
            ssgiParams.Distance = distance;
            ssgiParams.MaxRayCount = maxRayCount;
            ssgiParams.RaySteps = raySteps;
            ssgiParams.RayStep = rayStep;
            ssgiParamsBuffer = new(ssgiParams, CpuAccessFlags.Write);

            Unsafe.SkipInit<DenoiseParams>(out var denoiseParams);
            denoiseParams.SigmaS = sigmaS;
            denoiseParams.SigmaR = sigmaR;
            denoiseParams.Radius = radius;
            denoiseParamsBuffer = new(denoiseParams, CpuAccessFlags.Write);

            ssgiBuffer = creator.CreateBuffer("SSGI_BUFFER", creationFlags: ResourceCreationFlags.None);
            tempSsgiBuffer = new(creator.Format, creator.Width, creator.Height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(creator, "SSGI", radius: GaussianRadius.Radius7x7, additive: true);
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                Unsafe.SkipInit<SSGIParams>(out var ssgiParams);
                ssgiParams.Intensity = intensity;
                ssgiParams.Distance = distance;
                ssgiParams.MaxRayCount = maxRayCount;
                ssgiParams.RaySteps = raySteps;
                ssgiParams.RayStep = rayStep;
                ssgiParams.RayHitThreshold = rayHitThreshold;
                ssgiParamsBuffer.Update(context, ssgiParams);
                Unsafe.SkipInit<DenoiseParams>(out var denoiseParams);
                denoiseParams.SigmaS = sigmaS;
                denoiseParams.SigmaR = sigmaR;
                denoiseParams.Radius = radius;
                denoiseParamsBuffer.Update(context, denoiseParams);
                dirty = false;
            }
        }

        public override void UpdateBindings()
        {
            psoSSGI.Bindings.SetSRV("inputTex", Input);
            psoSSGI.Bindings.SetSRV("prevTex", tempSsgiBuffer.SRV);
            psoSSGI.Bindings.SetCBV("SSGIParams", ssgiParamsBuffer);
            psoDenoise.Bindings.SetSRV("inputTex", ssgiBuffer.Value);
            psoDenoise.Bindings.SetCBV("DenoiseParams", denoiseParamsBuffer);

            psoBlend.Bindings.SetSRV("inputTex", Input);
            psoBlend.Bindings.SetSRV("indirectTex", tempSsgiBuffer!.SRV);
            psoBlend.Bindings.SetCBV("SSGIParams", ssgiParamsBuffer);
        }

        public override void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            context.ClearRenderTargetView(ssgiBuffer.Value!, default);

            context.SetRenderTarget(ssgiBuffer.Value!, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(psoSSGI);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);

            context.SetRenderTarget(tempSsgiBuffer!, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(psoDenoise);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(psoBlend);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            creator.DisposeResource("SSGI_BUFFER");
            psoSSGI.Dispose();
            psoDenoise.Dispose();
            psoBlend.Dispose();
            ssgiParamsBuffer.Dispose();
            denoiseParamsBuffer.Dispose();
            blur.Dispose();
        }
    }
}