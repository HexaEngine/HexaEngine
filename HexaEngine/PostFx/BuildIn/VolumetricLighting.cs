namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public class VolumetricLighting : PostFxBase
    {
        private IGraphicsPipeline spotlightPipeline;
        private StructuredBuffer<CBVolumetricSpot> spotlights;

        private ISamplerState linearClampSampler;
        private ISamplerState shadowSampler;

        private Texture2D buffer;

        private ResourceRef<ShadowAtlas> shadowAtlas;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;

        private struct CBVolumetricSpot
        {
            public LightData Light;
            public ShadowData ShadowData;
            public float VolumetricStrength;

            public CBVolumetricSpot(LightData light, ShadowData shadowData, float volumetricStrength)
            {
                Light = light;
                ShadowData = shadowData;
                VolumetricStrength = volumetricStrength;
            }
        }

        /// <inheritdoc/>
        public override string Name { get; } = "VolumetricLighting";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; } = PostFxFlags.Inline | PostFxFlags.Dynamic;

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore("ColorGrading")
                .RunAfter("HBAO")
                .RunAfter("SSGI")
                .RunAfter("SSR")
                .RunBefore("MotionBlur")
                .RunBefore("AutoExposure")
                .RunBefore("TAA")
                .RunBefore("DepthOfField")
                .RunBefore("ChromaticAberration")
                .RunBefore("Bloom");
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            shadowAtlas = creator.GetShadowAtlas("ShadowAtlas");
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            spotlightPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/volumetric/spot.hlsl"
            },
            GraphicsPipelineState.DefaultAdditiveFullscreen, macros);

            spotlights = new(device, CpuAccessFlags.Write);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            shadowSampler = device.CreateSamplerState(SamplerStateDescription.ComparisonLinearBorder);

            buffer = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
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

            context.ClearRenderTargetView(buffer, default);

            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (!light.VolumetricsEnable || !light.ShadowMapEnable)
                {
                    continue;
                }

                var lightData = lights.LightBuffer[i];

                switch (light.LightType)
                {
                    case LightType.Point:
                        break;

                    case LightType.Spot:
                        DrawSpotlightVolumetric(context, lightData, ((Spotlight)light).GetShadowData(), light.VolumetricsMultiplier);
                        break;

                    case LightType.Directional:
                        break;
                }
            }
        }

        private void DrawSpotlightVolumetric(IGraphicsContext context, LightData light, ShadowData shadow, float strength)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);

            spotlights.ResetCounter();
            spotlights.Add(new(light, shadow, strength));
            spotlights.Update(context);

            context.PSSetConstantBuffer(1, camera.Value);

            context.PSSetShaderResource(0, depth.Value.SRV);
            context.PSSetShaderResource(1, shadowAtlas.Value.SRV);
            context.PSSetShaderResource(2, spotlights.SRV);

            context.PSSetSampler(0, linearClampSampler);
            context.PSSetSampler(1, shadowSampler);

            context.SetGraphicsPipeline(spotlightPipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            context.PSSetSampler(0, null);
            context.PSSetSampler(1, null);

            context.PSSetShaderResource(0, null);
            context.PSSetShaderResource(1, null);
            context.PSSetShaderResource(2, null);

            context.PSSetConstantBuffer(1, null);

            context.SetRenderTarget(null, null);
            context.SetViewport(default);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            spotlightPipeline.Dispose();
            spotlights.Dispose();
            linearClampSampler.Dispose();
            shadowSampler.Dispose();
            buffer.Dispose();
        }
    }
}