namespace HexaEngine.PostFx.BuildIn
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    [EditorDisplayName("Lens Flare")]
    public class LensFlare : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState downsamplePSO;
        private ConstantBuffer<DownsampleParams> downsampleCB;

        private ResourceRef<Texture2D> downsampleBuffer;
        private Viewport downsampleViewport;
        private ResourceRef<Texture2D> accumBuffer;

        private IGraphicsPipelineState lensPSO;
        private ConstantBuffer<LensParams> lensCB;

        private GaussianBlur blur;

        private IGraphicsPipelineState blendPipeline;
        private Texture2D lensDirt;
#nullable restore
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

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

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

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            downsamplePSO = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lensflare/downsample.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            downsampleCB = new(new DownsampleParams(scale, bias), CpuAccessFlags.Write);

            downsampleBuffer = creator.CreateBufferHalfRes("LENS_DOWNSAMPLE_BUFFER");
            downsampleViewport = creator.ViewportHalf;

            lensPSO = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lensflare/lens.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            lensCB = new(CpuAccessFlags.Write);

            accumBuffer = creator.CreateBufferHalfRes("LENS_ACCUMULATION_BUFFER");

            blur = new(creator, "LENS", additive: true);

            blendPipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lensflare/blend.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        public override void Update(IGraphicsContext context)
        {
            downsampleCB.Update(context, new(scale, bias));
            lensCB.Update(context, new(ghosts, ghostDispersal, downsampleViewport.Size, haloWidth, distortion));
        }

        public override void UpdateBindings()
        {
            downsamplePSO.Bindings.SetSRV("inputTexture", Input);
            downsamplePSO.Bindings.SetCBV("DownsampleParams", downsampleCB);

            lensPSO.Bindings.SetSRV("inputTexture", downsampleBuffer.Value!.SRV);
            lensPSO.Bindings.SetCBV("LensParams", lensCB);
        }

        public override void Draw(IGraphicsContext context)
        {
            // downsample and threshold
            context.BeginDraw(new(downsampleBuffer.Value!, null));
            context.SetGraphicsPipelineState(downsamplePSO);
            context.DrawInstanced(4, 1, 0, 0);

            // lens
            context.BeginDraw(new(accumBuffer.Value!, null));
            context.SetGraphicsPipelineState(lensPSO);
            context.DrawInstanced(4, 1, 0, 0);
        }

        public override void Compose(IGraphicsContext context)
        {
            // upscale, blur and blend
            blur.Blur(context, accumBuffer.Value!.SRV!, Output, accumBuffer.Value.Viewport.Width, accumBuffer.Value.Viewport.Height, Viewport.Width, Viewport.Height);
        }

        protected override void DisposeCore()
        {
            downsamplePSO.Dispose();
            downsampleCB.Dispose();
            lensPSO.Dispose();
            lensCB.Dispose();
            blur.Dispose();
            blendPipeline.Dispose();
        }
    }
}