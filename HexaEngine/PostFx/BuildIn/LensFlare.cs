namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class LensFlare : PostFxBase
    {
        private IGraphicsPipeline downsamplePipeline;
        private ConstantBuffer<DownsampleParams> downsampleCB;
        private ISamplerState samplerState;
        private ResourceRef<Texture2D> downsampleBuffer;
        private ResourceRef<Texture2D> accumBuffer;

        private IGraphicsPipeline lensPipeline;
        private ConstantBuffer<LensParams> lensCB;

        private GaussianBlur blur;

        private IGraphicsPipeline blendPipeline;
        private Texture2D lensDirt;

        private Vector4 scale = new(0.02f);
        private Vector4 bias = new(-3);
        private uint ghosts = 8;
        private float ghostDispersal = 0.37f;
        private float haloWidth = 0.47f;
        private float distortion = 1.5f;

        private struct DownsampleParams
        {
            public Vector4 Scale;
            public Vector4 Bias;

            public DownsampleParams(Vector4 scale, Vector4 bias)
            {
                Scale = scale;
                Bias = bias;
            }
        }

        private struct LensParams
        {
            public uint Ghosts;
            public float GhostDispersal;
            public Vector2 TextureSize;
            public float HaloWidth;
            public float Distortion;
            public Vector2 Padd;

            public LensParams(uint ghosts, float ghostDispersal, Vector2 textureSize, float haloWidth, float distortion)
            {
                Ghosts = ghosts;
                GhostDispersal = ghostDispersal;
                TextureSize = textureSize;
                HaloWidth = haloWidth;
                Distortion = distortion;
            }
        }

        public override string Name { get; } = "LensFlare";

        public override PostFxFlags Flags { get; } = PostFxFlags.Compose;

        public Vector4 Scale { get => scale; set => NotifyPropertyChangedAndSet(ref scale, value); }

        public Vector4 Bias { get => bias; set => NotifyPropertyChangedAndSet(ref bias, value); }

        public uint Ghosts { get => ghosts; set => NotifyPropertyChangedAndSet(ref ghosts, value); }

        public float GhostDispersal { get => ghostDispersal; set => NotifyPropertyChangedAndSet(ref ghostDispersal, value); }

        public float HaloWidth { get => haloWidth; set => NotifyPropertyChangedAndSet(ref haloWidth, value); }

        public float Distortion { get => distortion; set => NotifyPropertyChangedAndSet(ref distortion, value); }

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunBefore<Vignette>()
                .RunAfter<VolumetricClouds>()
                .RunAfter<VolumetricScattering>()
                .RunBefore<Bloom>()
                .RunBefore<TAA>()
                .RunBefore<MotionBlur>()
                .Compose()
                    .After<TAA>()
                    .After<MotionBlur>()
                    .Before<ColorGrading>()
                    .Before<Vignette>();
        }

        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            downsamplePipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lensflare/downsample.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });
            downsampleCB = new(device, CpuAccessFlags.Write);
            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            downsampleBuffer = creator.CreateTexture2D("LENS_DOWNSAMPLE_BUFFER", new(Format.R16G16B16A16Float, width / 2, height / 2, 1, 1, GpuAccessFlags.RW));

            lensPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lensflare/lens.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });
            lensCB = new(device, CpuAccessFlags.Write);

            accumBuffer = creator.CreateTexture2D("LENS_ACCUMULATION_BUFFER", new(Format.R16G16B16A16Float, width / 2, height / 2, 1, 1, GpuAccessFlags.RW));

            blur = new(device, Format.R16G16B16A16Float, width, height, false, true);

            blendPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lensflare/blend.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });
        }

        public override void Update(IGraphicsContext context)
        {
            downsampleCB.Update(context, new(scale, bias));
            lensCB.Update(context, new(ghosts, ghostDispersal, downsampleBuffer.Value.Viewport.Size, haloWidth, distortion));
        }

        public override void Draw(IGraphicsContext context)
        {
            // downsample and threshold
            context.SetGraphicsPipeline(downsamplePipeline);
            context.SetRenderTarget(downsampleBuffer.Value.RTV, null);
            context.SetViewport(downsampleBuffer.Value.Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetSampler(0, samplerState);
            context.PSSetConstantBuffer(0, downsampleCB);
            context.DrawInstanced(4, 1, 0, 0);

            // lens
            context.SetGraphicsPipeline(lensPipeline);
            context.SetRenderTarget(accumBuffer.Value.RTV, null);
            context.SetViewport(accumBuffer.Value.Viewport);
            context.PSSetShaderResource(0, downsampleBuffer.Value.SRV);
            context.PSSetConstantBuffer(0, lensCB);
            context.DrawInstanced(4, 1, 0, 0);
        }

        public override void Compose(IGraphicsContext context)
        {
            // upscale, blur and blend
            blur.Blur(context, accumBuffer.Value.SRV, Output, accumBuffer.Value.Viewport.Width, accumBuffer.Value.Viewport.Height, Viewport.Width, Viewport.Height);
        }

        protected override void DisposeCore()
        {
            downsamplePipeline.Dispose();
            downsampleCB.Dispose();
            samplerState.Dispose();
            lensPipeline.Dispose();
            lensCB.Dispose();
            blur.Dispose();
            blendPipeline.Dispose();
        }
    }
}