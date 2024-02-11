namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;

    /// <summary>
    /// Screen Space Reflections (SSR) post-processing effect.
    /// </summary>
    public class SSR : PostFxBase
    {
        private IGraphicsPipelineState pipelineSSR;

        private ISamplerState pointClampSampler;
        private ISamplerState linearClampSampler;
        private ISamplerState linearBorderSampler;

        private ConstantBuffer<SSRParams> ssrParamsBuffer;

        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<GBuffer> gbuffer;

        private SSRQualityPreset qualityPreset = SSRQualityPreset.Medium;
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
        /// Gets or sets the quality preset for SSR.
        /// </summary>
        public SSRQualityPreset QualityPreset
        {
            get => qualityPreset;
            set
            {
                NotifyPropertyChangedAndSetAndReload(ref qualityPreset, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of rays for SSR.
        /// </summary>
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
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            gbuffer = creator.GetGBuffer("GBuffer");

            pointClampSampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            linearBorderSampler = device.CreateSamplerState(SamplerStateDescription.LinearBorder);

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
            if (qualityPreset == SSRQualityPreset.Dynamic)
            {
                ssrParamsBuffer = new(device, CpuAccessFlags.Write);
            }

            pipelineSSR = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssr/ps.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        /// <inheritdoc/>
        public override unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
                return;

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            if (ssrParamsBuffer != null)
            {
                SSRParams ssrParams;
                ssrParams.MaxRayCount = maxRayCount;
                ssrParams.RaySteps = raySteps;
                ssrParams.RayStep = rayStep;
                ssrParams.RayHitThreshold = rayHitThreshold;
                ssrParamsBuffer.Update(context, ssrParams);
                nint* cbvs = stackalloc nint[] { ssrParamsBuffer.NativePointer, camera.Value.NativePointer };
                context.PSSetConstantBuffers(0, 2, (void**)cbvs);
            }
            else
            {
                context.PSSetConstantBuffer(1, camera.Value);
            }

            nint* srvs = stackalloc nint[] { depth.Value.SRV.NativePointer, gbuffer.Value.SRVs[1].NativePointer, Input.NativePointer, gbuffer.Value.SRVs[2].NativePointer };
            context.PSSetShaderResources(0, 4, (void**)srvs);

            nint* smps = stackalloc nint[] { pointClampSampler.NativePointer, linearClampSampler.NativePointer, linearBorderSampler.NativePointer };
            context.PSSetSamplers(0, 3, (void**)smps);

            context.SetPipelineState(pipelineSSR);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);

            nint* emptySmps = stackalloc nint[3];
            context.PSSetSamplers(0, 3, (void**)emptySmps);

            nint* emptySrvs = stackalloc nint[4];
            context.PSSetShaderResources(0, 4, (void**)emptySrvs);

            nint* emptyCbvs = stackalloc nint[2];
            context.PSSetConstantBuffers(0, 2, (void**)emptyCbvs);

            context.SetRenderTarget(null, null);
            context.SetViewport(default);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipelineSSR.Dispose();
            pointClampSampler.Dispose();
            linearClampSampler.Dispose();
            linearBorderSampler.Dispose();
            ssrParamsBuffer?.Dispose();
        }
    }
}