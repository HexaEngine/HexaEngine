namespace HexaEngine.PostFx.BuildIn
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Weather;
    using System.Numerics;

    [EditorDisplayName("Volumetric Clouds")]
    public class VolumetricClouds : PostFxBase
    {
#nullable disable
        private IGraphicsDevice device;
        private IGraphicsPipelineState pipeline;
        private ISamplerState linearWrapSampler;
        private ISamplerState pointWrapSampler;

        private Texture2D weatherTex;
        private Texture3D cloudTex;
        private Texture3D worleyTex;

        private Texture2D intermediateTex;
        private GaussianBlur gaussianBlur;

        private ResourceRef<DepthStencil> depth;
        private ResourceRef<DepthMipChain> depthMip;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<ConstantBuffer<CBWeather>> weather;
        private ResourceRef<GBuffer> gbuffer;
#nullable restore

        public override string Name { get; } = "VolumetricClouds";

        public override PostFxFlags Flags { get; } = PostFxFlags.Inline | PostFxFlags.Dynamic;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                 .RunBefore<ColorGrading>()
                 .RunBefore<Vignette>()
                 .RunBefore<SSR>()
                 .RunBefore<TAA>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            depthMip = creator.GetDepthMipChain("HiZBuffer");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            weather = creator.GetConstantBuffer<CBWeather>("CBWeather");
            gbuffer = creator.GetGBuffer("GBuffer");

            this.device = device;
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/clouds/ps.hlsl",
            }, new()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);
            pointWrapSampler = device.CreateSamplerState(SamplerStateDescription.PointWrap);

            weatherTex = new(new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/weather.dds"));
            cloudTex = new(new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/cloud.dds"));
            worleyTex = new(new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/worley.dds"));

            intermediateTex = new(Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            gaussianBlur = new(creator, "VOLUMETRIC_CLOUDS", alphaBlend: true);
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("weatherTex", weatherTex);
            pipeline.Bindings.SetSRV("cloudTex", cloudTex);
            pipeline.Bindings.SetSRV("worleyTex", worleyTex);
            pipeline.Bindings.SetSRV("depthTex", depthMip.Value!.SRV);
            pipeline.Bindings.SetCBV("CameraBuffer", camera.Value);
            pipeline.Bindings.SetCBV("WeatherBuffer", weather.Value);
            pipeline.Bindings.SetSampler("linearWrapSampler", linearWrapSampler);
            pipeline.Bindings.SetSampler("pointWrapSampler", pointWrapSampler);
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null || WeatherSystem.Current == null)
            {
                return;
            }

            context.ClearRenderTargetView(intermediateTex.RTV!, default);
            context.SetRenderTarget(intermediateTex.RTV, depth.Value);
            context.SetViewport(Viewport);

            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipelineState(null);

            gaussianBlur.Blur(context, intermediateTex.SRV!, Output, (int)Viewport.Width, (int)Viewport.Height);
        }

        public override void Resize(int width, int height)
        {
            intermediateTex.Resize(Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            gaussianBlur.Resize(Format.R16G16B16A16Float, width, height);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            linearWrapSampler.Dispose();
            pointWrapSampler.Dispose();
            weatherTex.Dispose();
            cloudTex.Dispose();
            worleyTex.Dispose();

            intermediateTex.Dispose();
            gaussianBlur.Dispose();
        }
    }
}