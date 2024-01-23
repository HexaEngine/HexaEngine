namespace HexaEngine.PostFx.BuildIn
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics.Noise;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Weather;
    using System.Numerics;

    public class LensFlare : PostFxBase
    {
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<Texture2D> sunMask;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<ConstantBuffer<CBWeather>> weather;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;
        private ConstantBuffer<LensFlareParams> paramsBuffer;

        public struct LensFlareParams
        {
            public Vector4 SunPosition;
            public Vector4 Tint;

            public LensFlareParams(Vector4 sunPosition, Vector4 tint)
            {
                SunPosition = sunPosition;
                Tint = tint;
            }
        }

        public override string Name => "LensFlare";

        public override PostFxFlags Flags { get; } = PostFxFlags.Inline | PostFxFlags.Dynamic;

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunBefore<Vignette>()
                .RunAfter<VolumetricClouds>()
                .RunAfter<VolumetricScattering>()
                .RunAfter<Bloom>();
        }

        public override unsafe void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            sunMask = creator.GetTexture2D("SunMask");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lensflare/ps.hlsl",
                State = GraphicsPipelineState.DefaultAdditiveFullscreen,
                Macros = macros
            });

            paramsBuffer = new(device, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            Viewport = new(width, height);
        }

        public override void Resize(int width, int height)
        {
        }

        public override void Update(IGraphicsContext context)
        {
        }

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
            {
                return;
            }

            var camera = CameraManager.Current;
            var lights = LightManager.Current;
            if (camera == null || lights == null)
            {
                return;
            }

            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (light is DirectionalLight)
                {
                    Vector3 sunPosition = Vector3.Transform(light.Transform.Backward * camera.Transform.ClipRange + camera.Transform.GlobalPosition, camera.Transform.View);

                    // skip render, the light is behind the camera.
                    if (sunPosition.Z < 0.0f)
                    {
                        return;
                    }

                    var screenPos = camera.ProjectPosition(sunPosition);

                    LensFlareParams lensFlare;
                    lensFlare.SunPosition = new(screenPos.X, screenPos.Y, screenPos.Z, 1);
                    lensFlare.Tint = new Vector4(1.4f, 1.2f, 1.0f, 1);

                    paramsBuffer.Update(context, lensFlare);

                    context.SetRenderTarget(Output, default);
                    context.SetViewport(Viewport);
                    context.PSSetConstantBuffer(0, paramsBuffer);
                    context.PSSetConstantBuffer(1, this.camera.Value);
                    context.PSSetConstantBuffer(2, weather.Value);
                    context.PSSetShaderResource(0, depth.Value);
                    context.PSSetShaderResource(2, sunMask.Value);
                    context.PSSetSampler(0, sampler);
                    context.SetGraphicsPipeline(pipeline);
                    context.DrawInstanced(4, 1, 0, 0);
                    context.ClearState();

                    break;
                }
            }
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            sampler.Dispose();
            paramsBuffer.Dispose();
        }
    }
}