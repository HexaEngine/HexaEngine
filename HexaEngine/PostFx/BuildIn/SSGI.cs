namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    [EditorDisplayName("SSGI")]
    public class SSGI : PostFxBase
    {
        private PostFxGraphResourceBuilder creator;
        private IGraphicsPipelineState psoSSGI;
        private ConstantBuffer<SSGIParams> ssgiParamsBuffer;
        private ResourceRef<Texture2D> ssgiBuffer;
        private GaussianBlur blur;
        private ISamplerState linearWrapSampler;

        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<GBuffer> gbuffer;

        private float intensity = 1;
        private float distance = 10;
        private int maxRayCount = 16;
        private int raySteps = 8;
        private float depthBias = 0.1f;
        private SSGIQualityPreset qualityPreset;

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
        /// Gets or sets the depth bias for SSGI.
        /// </summary>
        [EditorProperty("Depth Bias")]
        [Tooltip("Specifies the depth bias for Screen Space Global Illumination (SSGI). This value is applicable when the quality preset is set to 'Custom'.")]
        public float DepthBias
        {
            get => depthBias;
            set
            {
                NotifyPropertyChangedAndSet(ref depthBias, value);
                if (qualityPreset == SSGIQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        #region Structs

        private struct SSGIParams
        {
            public float Intensity;
            public float Distance;
            public int MaxRayCount;
            public int RaySteps;
            public float DepthBias;
            public Vector3 Padding;

            public SSGIParams()
            {
                Intensity = 1f;
                Distance = 10;
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
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            gbuffer = creator.GetGBuffer("GBuffer");

            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("SSGI_QUALITY", ((int)qualityPreset).ToString())
            };
            if (qualityPreset == SSGIQualityPreset.Custom)
            {
                shaderMacros.Add(new("SSGI_RAY_COUNT", maxRayCount));
                shaderMacros.Add(new("SSGI_RAY_STEPS", raySteps));
                shaderMacros.Add(new("SSGI_DEPTH_BIAS", depthBias));
            }

            psoSSGI = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssgi/ps.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            Unsafe.SkipInit<SSGIParams>(out var ssgiParams);
            ssgiParams.Intensity = intensity;
            ssgiParams.Distance = distance;
            ssgiParams.MaxRayCount = maxRayCount;
            ssgiParams.RaySteps = raySteps;
            ssgiParams.DepthBias = depthBias;
            ssgiParamsBuffer = new(ssgiParams, CpuAccessFlags.Write);
            ssgiBuffer = creator.CreateBuffer("SSGI_BUFFER");
            blur = new(creator, "SSGI", additive: true);
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
                ssgiParams.DepthBias = depthBias;
                ssgiParamsBuffer.Update(context, ssgiParams);
                dirty = false;
            }
        }

        public override void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            context.ClearRenderTargetView(ssgiBuffer.Value, default);

            context.SetRenderTarget(ssgiBuffer.Value, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetShaderResource(1, depth.Value.SRV);
            context.PSSetShaderResource(2, gbuffer.Value.SRVs[1]);
            context.PSSetConstantBuffer(0, ssgiParamsBuffer);
            context.PSSetConstantBuffer(1, camera.Value);
            context.PSSetSampler(0, linearWrapSampler);

            context.SetPipelineState(psoSSGI);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetPipelineState(null);

            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(1, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(1, null);
            context.PSSetShaderResource(0, null);
            context.SetRenderTarget(null, null);

            blur.Blur(context, ssgiBuffer.Value, Output, Viewport.Width, Viewport.Height);
        }

        protected override void DisposeCore()
        {
            creator.DisposeResource("SSGI_BUFFER");
            psoSSGI.Dispose();
            linearWrapSampler.Dispose();
            ssgiParamsBuffer.Dispose();
            blur.Dispose();
        }
    }
}