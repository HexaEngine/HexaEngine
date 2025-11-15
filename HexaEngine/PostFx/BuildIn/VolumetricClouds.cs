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

        private Texture2D weatherTex;
        private Texture3D cloudTex;
        private Texture3D worleyTex;

        private Texture2D intermediateTex;
        private GaussianBlur gaussianBlur;

        private ResourceRef<DepthStencil> depth;
        private ResourceRef<DepthMipChain> depthMip;

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

            this.device = device;
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/effects/clouds/ps.hlsl",
            }, new()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            weatherTex = new(new TextureFileDescription(new("HexaEngine.Core:textures/clouds/weather.dds")));
            cloudTex = new(new TextureFileDescription(new("HexaEngine.Core:textures/clouds/cloud.dds")));
            worleyTex = new(new TextureFileDescription(new("HexaEngine.Core:textures/clouds/worley.dds")));

            intermediateTex = new(Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            gaussianBlur = new(creator, "VOLUMETRIC_CLOUDS", alphaBlend: true);
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("weatherTex", weatherTex);
            pipeline.Bindings.SetSRV("cloudTex", cloudTex);
            pipeline.Bindings.SetSRV("worleyTex", worleyTex);
            pipeline.Bindings.SetSRV("depthTex", depthMip.Value!.SRV);
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
            weatherTex.Dispose();
            cloudTex.Dispose();
            worleyTex.Dispose();

            intermediateTex.Dispose();
            gaussianBlur.Dispose();
        }
    }
}