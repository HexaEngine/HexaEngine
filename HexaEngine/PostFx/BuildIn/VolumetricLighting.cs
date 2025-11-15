namespace HexaEngine.PostFx.BuildIn
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using System.Numerics;

    [EditorDisplayName("Volumetric Lighting")]
    public class VolumetricLighting : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;

        private ConstantBuffer<VolumetricLightingConstants> constantBuffer;
        private StructuredBuffer<VolumetricLightData> volumetricLightBuffer;

        private IGraphicsPipelineState blurPipeline;
        private ConstantBuffer<BlurParams> blurParams;

        private ISamplerState shadowSampler;

        private ResourceRef<Texture2D> buffer;
        private PostFxGraphResourceBuilder creator;

        private ResourceRef<DepthMipChain> depthChain;

#nullable restore

        private VolumetricLightingQualityPreset qualityPreset = VolumetricLightingQualityPreset.Medium;
        private int sampleCount;
        private float density = 0.0015f;
        private float rayleighCoefficient = 1.0f;
        private float mieCoefficient = 1.0f;
        private float mieG = 0.1f;

        public enum VolumetricLightingQualityPreset
        {
            Custom = -1,
            Low = 0,
            Medium = 1,
            High = 2,
            Extreme = 3,
        }

        private struct VolumetricLightingConstants
        {
            public uint LightCount;
            public UPoint3 Padd0;
            public float Density;
            public float RayleighCoefficient;
            public float MieCoefficient;
            public float MieG;

            public VolumetricLightingConstants(uint lightCount)
            {
                LightCount = lightCount;
                Padd0 = default;
                Density = 0.0015f;
                RayleighCoefficient = 1f;
                MieCoefficient = 1f;
                MieG = 0.1f;
            }

            public VolumetricLightingConstants(uint lightCount, float density, float rayleighCoefficient, float mieCoefficient, float mieG)
            {
                LightCount = lightCount;
                Padd0 = default;
                Density = density;
                RayleighCoefficient = rayleighCoefficient;
                MieCoefficient = mieCoefficient;
                MieG = mieG;
            }
        }

        private struct VolumetricLightData
        {
            public LightData Light;
            public float VolumetricStrength;

            public VolumetricLightData(LightData light, float volumetricStrength)
            {
                Light = light;
                VolumetricStrength = volumetricStrength;
            }
        }

        private struct BlurParams
        {
            public Vector2 ScreenSize;
            public Vector2 SourceSize;

            public BlurParams(Vector2 screenSize, Vector2 sourceSize)
            {
                ScreenSize = screenSize;
                SourceSize = sourceSize;
            }
        }

        /// <inheritdoc/>
        public override string Name { get; } = "VolumetricLighting";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; } = PostFxFlags.Inline | PostFxFlags.Dynamic;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        [EditorProperty<VolumetricLightingQualityPreset>("Quality Preset")]
        public VolumetricLightingQualityPreset QualityPreset
        {
            get => qualityPreset;
            set => NotifyPropertyChangedAndSetAndReload(ref qualityPreset, value);
        }

        [EditorProperty("Sample Count")]
        public int SampleCount
        {
            get => sampleCount;
            set
            {
                NotifyPropertyChangedAndSet(ref sampleCount, value);
                if (qualityPreset == VolumetricLightingQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        [EditorProperty("Density")]
        public float Density
        {
            get => density;
            set => NotifyPropertyChangedAndSet(ref density, value);
        }

        [EditorProperty("MieG")]
        public float MieG
        {
            get => mieG;
            set => NotifyPropertyChangedAndSet(ref mieG, value);
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunAfter<HBAO>()
                .RunAfter<SSGI>()
                .RunBefore<SSR>()
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
            this.creator = creator;

            depthChain = creator.GetDepthMipChain("HiZBuffer");

            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("VOLUMETRIC_LIGHT_QUALITY", ((int)qualityPreset).ToString())
            };
            if (qualityPreset == VolumetricLightingQualityPreset.Custom)
            {
                shaderMacros.Add(new("SAMPLE_COUNT", sampleCount));
            }

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
                PixelShader = "HexaEngine.PostFx:shaders/effects/volumetric/ps.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            constantBuffer = new(CpuAccessFlags.Write);

            blurPipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
                PixelShader = "HexaEngine.PostFx:shaders/effects/volumetric/blur.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultAdditiveFullscreen);
            blurParams = new(CpuAccessFlags.Write);

            volumetricLightBuffer = new(CpuAccessFlags.Write);
            shadowSampler = device.CreateSamplerState(SamplerStateDescription.ComparisonLinearBorder);

            buffer = creator.CreateBufferHalfRes("VOLUMETRIC_LIGHTNING_BUFFER");

            LightManager.ActiveLightsChanged += ActiveLightsChanged;
            LightManager.LightUpdated += LightUpdated;

            if (LightManager.Current != null)
            {
                LightUpdated(LightManager.Current, new(LightManager.Current, null!));
                ActiveLightsChanged(LightManager.Current, new(LightManager.Current));
            }
        }

        private void LightUpdated(object? sender, LightUpdatedEventArgs e)
        {
            var lights = e.LightManager;
            volumetricLightBuffer.ResetCounter();
            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var lightSource = lights.Active[i];
                if (lightSource is not Light light)
                {
                    continue;
                }

                if (!light.VolumetricsEnable)
                {
                    continue;
                }

                volumetricLightBuffer.Add(new(new(light), light.VolumetricsMultiplier));

                if (light is DirectionalLight dir)
                {
                    pipeline.Bindings.SetSRV("cascadeShadowMaps", dir.GetShadowMap());
                }
            }

            NotifyPropertyChanged();
        }

        private void ActiveLightsChanged(object? sender, ActiveLightsChangedEventArgs e)
        {
            pipeline.Bindings.SetSRV("shadowData", e.LightManager.ShadowDataBuffer.SRV);
            NotifyPropertyChanged();
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetCBV("VolumetricParams", constantBuffer);

            pipeline.Bindings.SetSRV("lights", volumetricLightBuffer.SRV);

            blurPipeline.Bindings.SetSRV("depthTex", depthChain.Value!.SRV);
            blurPipeline.Bindings.SetSRV("volumetricLightTex", buffer.Value);

            blurPipeline.Bindings.SetCBV("BlurParams", blurParams);
        }

        public override void Update(IGraphicsContext context)
        {
            if (volumetricLightBuffer.IsEmpty) return;

            volumetricLightBuffer.Update(context);
            constantBuffer.Update(context, new(volumetricLightBuffer.Count, density, rayleighCoefficient, mieCoefficient, mieG));

            blurParams.Update(context, new(Viewport.Size, buffer.Value!.Viewport.Size));
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context)
        {
            if (volumetricLightBuffer.IsEmpty) return;

            // Volumetric pass
            context.SetRenderTarget(buffer.Value, null);
            context.SetViewport(buffer.Value!.Viewport);

            context.SetGraphicsPipelineState(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipelineState(null);

            // Blur Pass
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);

            context.SetGraphicsPipelineState(blurPipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipelineState(null);

            context.SetRenderTarget(null, null);
            context.SetViewport(default);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            LightManager.ActiveLightsChanged -= ActiveLightsChanged;
            LightManager.LightUpdated -= LightUpdated;
            pipeline.Dispose();
            blurPipeline.Dispose();
            blurParams.Dispose();
            constantBuffer.Dispose();
            volumetricLightBuffer.Dispose();
            shadowSampler.Dispose();
        }
    }
}