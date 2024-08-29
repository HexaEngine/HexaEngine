namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;

    [EditorDisplayName("TAA")]
    public class TAA : PostFxBase
    {
        private PostFxGraphResourceBuilder creator;
        private IGraphicsPipelineState pipeline;
        private ISamplerState sampler;

        private ConstantBuffer<TAAParams> paramsBuffer;

        private ResourceRef<Texture2D> Velocity;
        private ResourceRef<Texture2D> Previous;

        private float alpha = 0.1f;
        private float colorBoxSigma = 0.3f;
        private bool antiFlicker = true;

        public override string Name => "TAA";

        public override PostFxFlags Flags => PostFxFlags.None;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        public float Alpha
        {
            get => alpha;
            set => NotifyPropertyChangedAndSet(ref alpha, value);
        }

        public float ColorBoxSigma
        {
            get => colorBoxSigma;
            set => NotifyPropertyChangedAndSet(ref colorBoxSigma, value);
        }

        public bool AntiFlicker
        {
            get => antiFlicker;
            set => NotifyPropertyChangedAndSet(ref antiFlicker, value);
        }

        private struct TAAParams
        {
            public float Alpha;
            public float ColorBoxSigma;
            public int AntiFlicker;
            public float padd;

            public TAAParams(float alpha, float colorBoxSigma, bool antiFlicker)
            {
                Alpha = alpha;
                ColorBoxSigma = colorBoxSigma;
                AntiFlicker = antiFlicker ? 1 : 0;
            }
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .AddBinding("VelocityBuffer")
                .RunBefore<ColorGrading>()
                .RunAfter<HBAO>()
                .RunAfter<SSGI>()
                .RunAfter<SSR>()
                .RunAfter<MotionBlur>()
                .RunAfter<AutoExposure>()
                .RunBefore<DepthOfField>()
                .RunBefore<ChromaticAberration>()
                .RunBefore<Bloom>()
                .Override<FXAA>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/taa/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            paramsBuffer = new(CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = creator.GetTexture2D("VelocityBuffer");
            Previous = creator.CreateBuffer("TAA_PREVIOUS_BUFFER", creationFlags: ResourceCreationFlags.None);

            Viewport = new(width, height);
        }

        public override void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(alpha, colorBoxSigma, antiFlicker));
                dirty = false;
            }
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            nint* srvs = stackalloc nint[3];
            srvs[0] = Input.NativePointer;
            srvs[1] = Velocity.Value.SRV.NativePointer;
            srvs[2] = Previous.Value.SRV.NativePointer;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, sampler);

            context.SetGraphicsPipelineState(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipelineState(null);

            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            ZeroMemory(srvs, sizeof(nint) * 3);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.SetRenderTarget(null, default);
            context.CopyResource(Previous.Value, OutputResource);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            sampler.Dispose();
            paramsBuffer.Dispose();
            creator.DisposeResource("Previous");
        }
    }
}