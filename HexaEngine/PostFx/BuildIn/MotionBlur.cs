namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;

    [EditorDisplayName("Motion Blur")]
    public class MotionBlur : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<MotionBlurParams> paramsBuffer;
        private ISamplerState sampler;
        private ResourceRef<Texture2D> Velocity;
#nullable restore
        private MotionBlurQualityPreset qualityPreset = MotionBlurQualityPreset.High;
        private float strength = 1;
        private int sampleCount = 16;

        public enum MotionBlurQualityPreset
        {
            Custom = -1,
            Low = 0,
            Medium = 1,
            High = 2,
            Ultra = 3,
        }

        private struct MotionBlurParams
        {
            public float Strength;
            public Vector3 Padding;

            public MotionBlurParams(float strength)
            {
                Strength = strength;
                Padding = default;
            }
        }

        public override string Name => "MotionBlur";

        public override PostFxFlags Flags { get; }

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        public MotionBlurQualityPreset QualityPreset
        {
            get => qualityPreset;
            set => NotifyPropertyChangedAndSetAndReload(ref qualityPreset, value);
        }

        public float Strength
        {
            get => strength;
            set => NotifyPropertyChangedAndSet(ref strength, value);
        }

        public int SampleCount
        {
            get => sampleCount;
            set
            {
                NotifyPropertyChangedAndSet(ref sampleCount, value);
                if (qualityPreset == MotionBlurQualityPreset.Custom)
                {
                    NotifyReload();
                }
            }
        }

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
              .AddBinding("VelocityBuffer")
              .RunBefore<ColorGrading>()
              .RunAfter<HBAO>()
              .RunAfter<SSGI>()
              .RunAfter<SSR>()
              .RunAfter<VolumetricLighting>()
              .RunBefore<AutoExposure>()
              .RunBefore<TAA>()
              .RunBefore<DepthOfField>()
              .RunBefore<ChromaticAberration>()
              .RunBefore<Bloom>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("MOTION_BLUR_QUALITY", ((int)qualityPreset).ToString())
            };

            if (qualityPreset == MotionBlurQualityPreset.Custom)
            {
                shaderMacros.Add(new("SAMPLE_COUNT", sampleCount));
            }

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/motionblur/ps.hlsl",
                Macros = [.. shaderMacros]
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            paramsBuffer = new(CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = creator.GetTexture2D("VelocityBuffer");

            Viewport = new(width, height);
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(strength));
                dirty = false;
            }
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("sceneTexture", Input);
            pipeline.Bindings.SetSRV("velocityBuffer", Velocity.Value);
            pipeline.Bindings.SetCBV("MotionBlurCB", paramsBuffer);
            pipeline.Bindings.SetSampler("linearWrapSampler", sampler);
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            sampler.Dispose();
        }
    }
}