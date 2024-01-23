namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class VolumetricLighting : PostFxBase
    {
        private IGraphicsPipeline pipeline;

        private ConstantBuffer<VolumetricLightingConstants> constantBuffer;
        private StructuredBuffer<VolumetricLightData> volumetricLightBuffer;

        private IGraphicsPipeline blurPipeline;
        private ConstantBuffer<BlurParams> blurParams;

        private ISamplerState linearClampSampler;
        private ISamplerState shadowSampler;

        private Texture2D buffer;

        private ResourceRef<ShadowAtlas> shadowAtlas;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<DepthMipChain> depthChain;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
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

        public VolumetricLightingQualityPreset QualityPreset
        {
            get => qualityPreset;
            set => NotifyPropertyChangedAndSetAndReload(ref qualityPreset, value);
        }

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

        public float Density
        {
            get => density;
            set => NotifyPropertyChangedAndSet(ref density, value);
        }

        public float RayleighCoefficient
        {
            get => rayleighCoefficient;
            set => NotifyPropertyChangedAndSet(ref rayleighCoefficient, value);
        }

        public float MieCoefficient
        {
            get => mieCoefficient;
            set => NotifyPropertyChangedAndSet(ref mieCoefficient, value);
        }

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
                .RunAfter<SSR>()
                .RunBefore<MotionBlur>()
                .RunBefore<AutoExposure>()
                .RunBefore<TAA>()
                .RunBefore<DepthOfField>()
                .RunBefore<ChromaticAberration>()
                .RunBefore<Bloom>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            shadowAtlas = creator.GetShadowAtlas("ShadowAtlas");
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            depthChain = creator.GetDepthMipChain("HiZBuffer");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("VOLUMETRIC_LIGHT_QUALITY", ((int)qualityPreset).ToString())
            };
            if (qualityPreset == VolumetricLightingQualityPreset.Custom)
            {
                shaderMacros.Add(new("SAMPLE_COUNT", sampleCount));
            }

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/volumetric/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = [.. shaderMacros]
            });
            constantBuffer = new(device, CpuAccessFlags.Write);

            blurPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/volumetric/blur.hlsl",
                State = GraphicsPipelineState.DefaultAdditiveFullscreen,
                Macros = [.. shaderMacros]
            });
            blurParams = new(device, CpuAccessFlags.Write);

            volumetricLightBuffer = new(device, CpuAccessFlags.Write);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            shadowSampler = device.CreateSamplerState(SamplerStateDescription.ComparisonLinearBorder);

            buffer = new(device, Format.R16G16B16A16Float, width / 2, height / 2, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
        }

        /// <inheritdoc/>
        public override void Update(IGraphicsContext context)
        {
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            var lights = LightManager.Current;

            if (lights == null)
            {
                return;
            }

            volumetricLightBuffer.ResetCounter();
            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (!light.VolumetricsEnable)
                {
                    continue;
                }

                var lightData = lights.LightBuffer[i];

                volumetricLightBuffer.Add(new(lightData, light.VolumetricsMultiplier));

                if (light is DirectionalLight dir)
                    context.PSSetShaderResource(2, dir.GetShadowMap());
            }

            // early exit nothing to render
            if (volumetricLightBuffer.Count == 0)
            {
                return;
            }

            // update buffers
            volumetricLightBuffer.Update(context);
            constantBuffer.Update(context, new(volumetricLightBuffer.Count, density, rayleighCoefficient, mieCoefficient, mieG));

            // Volumetric pass

            context.SetRenderTarget(buffer, null);
            context.SetViewport(buffer.Viewport);

            context.PSSetConstantBuffer(0, constantBuffer);
            context.PSSetConstantBuffer(1, camera.Value);

            context.PSSetShaderResource(0, depth.Value.SRV);
            context.PSSetShaderResource(1, shadowAtlas.Value.SRV);
            context.PSSetShaderResource(3, volumetricLightBuffer.SRV);
            context.PSSetShaderResource(4, lights.ShadowDataBuffer.SRV);

            context.PSSetSampler(0, linearClampSampler);
            context.PSSetSampler(1, shadowSampler);

            context.SetGraphicsPipeline(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            context.PSSetSampler(0, null);
            context.PSSetSampler(1, null);

            context.PSSetShaderResource(2, null);
            context.PSSetShaderResource(3, null);
            context.PSSetShaderResource(4, null);

            context.PSSetConstantBuffer(0, null);
            context.PSSetConstantBuffer(1, null);

            // Blur Pass

            blurParams.Update(context, new(Viewport.Size, buffer.Viewport.Size));

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);

            context.PSSetConstantBuffer(0, blurParams);

            context.PSSetShaderResource(0, depthChain.Value.SRV);
            context.PSSetShaderResource(1, buffer);

            context.SetGraphicsPipeline(blurPipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            context.PSSetShaderResource(0, null);
            context.PSSetShaderResource(1, null);

            context.PSSetConstantBuffer(0, null);

            context.SetRenderTarget(null, null);
            context.SetViewport(default);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipeline.Dispose();
            blurPipeline.Dispose();
            blurParams.Dispose();
            constantBuffer.Dispose();
            volumetricLightBuffer.Dispose();
            linearClampSampler.Dispose();
            shadowSampler.Dispose();
            buffer.Dispose();
        }
    }
}