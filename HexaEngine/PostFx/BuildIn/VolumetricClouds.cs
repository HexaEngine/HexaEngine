namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Weather;
    using System.Numerics;

    public class VolumetricClouds : PostFxBase
    {
        private IGraphicsDevice device;
        private IGraphicsPipeline pipeline;
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
                 .RunAfter<SSR>()
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
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/clouds/ps.hlsl",
                State = new()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.One,
                    Topology = PrimitiveTopology.TriangleStrip
                }
            });

            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);
            pointWrapSampler = device.CreateSamplerState(SamplerStateDescription.PointWrap);

            weatherTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/weather.dds"));
            cloudTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/cloud.dds"));
            worleyTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/worley.dds"));

            intermediateTex = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            gaussianBlur = new(creator, "VOLUMETRIC_CLOUDS", true);
        }

        public override void Update(IGraphicsContext context)
        {
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null || WeatherManager.Current == null || !WeatherManager.Current.HasSun)
            {
                return;
            }

            context.ClearRenderTargetView(intermediateTex.RTV, default);
            context.SetRenderTarget(intermediateTex.RTV, depth.Value);
            context.SetViewport(Viewport);
            nint* srvs = stackalloc nint[] { weatherTex.SRV.NativePointer, cloudTex.SRV.NativePointer, worleyTex.SRV.NativePointer, depthMip.Value.SRV.NativePointer };
            context.PSSetShaderResources(0, 4, (void**)srvs);
            nint* smps = stackalloc nint[] { linearWrapSampler.NativePointer, pointWrapSampler.NativePointer };
            context.PSSetSamplers(0, 2, (void**)smps);
            nint* cbcs = stackalloc nint[] { camera.Value.NativePointer, weather.Value.NativePointer };
            context.PSSetConstantBuffers(1, 2, (void**)cbcs);

            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);

            context.ClearState();

            gaussianBlur.Blur(context, intermediateTex.SRV, Output, (int)Viewport.Width, (int)Viewport.Height);
        }

        public override void Resize(int width, int height)
        {
            intermediateTex.Resize(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
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