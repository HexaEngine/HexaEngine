namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class MotionBlur : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<MotionBlurParams> paramsBuffer;
        private ISamplerState sampler;

        private ResourceRef<Texture2D> Velocity;

        private MotionBlurQualityPreset qualityPreset = MotionBlurQualityPreset.High;
        private float strength = 1;
        private int sampleCount = 16;

        public enum MotionBlurQualityPreset
        {
            Custom = -1,
            Low = 0,
            Medium = 1,
            High = 2,
            Extreme = 3,
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

        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("MOTION_BLUR_QUALITY", ((int)qualityPreset).ToString())
            };

            if (qualityPreset == MotionBlurQualityPreset.Custom)
            {
                shaderMacros.Add(new("SAMPLE_COUNT", sampleCount));
            }

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/motionblur/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = [.. shaderMacros]
            });
            paramsBuffer = new(device, CpuAccessFlags.Write);
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

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            nint* srvs = stackalloc nint[2];
            srvs[0] = Input.NativePointer;
            srvs[1] = Velocity.Value.SRV.NativePointer;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);

            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetSampler(0, sampler);

            context.PSSetConstantBuffer(0, paramsBuffer);

            context.SetGraphicsPipeline(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            context.PSSetConstantBuffer(0, null);

            context.PSSetSampler(0, null);
            ZeroMemory(srvs, sizeof(nint) * 2);
            context.PSSetShaderResources(0, 2, (void**)srvs);

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