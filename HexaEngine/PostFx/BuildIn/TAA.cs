namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

    public class TAA : PostFxBase, IAntialiasing
    {
        private GraphResourceBuilder creator;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        private ConstantBuffer<TAAParams> paramsBuffer;

        private ResourceRef<Texture2D> Velocity;
        private ResourceRef<Texture2D> Previous;

        private float alpha = 0.1f;
        private float colorBoxSigma = 1f;
        private bool antiFlicker = true;

        public override string Name => "TAA";

        public override PostFxFlags Flags => PostFxFlags.None;

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
                .RunBefore("ColorGrading")
                .RunAfter("HBAO")
                .RunAfter("SSGI")
                .RunAfter("SSR")
                .RunAfter("MotionBlur")
                .RunAfter("AutoExposure")
                .RunBefore("DepthOfField")
                .RunBefore("ChromaticAberration")
                .RunBefore("Bloom");
        }

        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/taa/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);

            paramsBuffer = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = creator.GetTexture2D("VelocityBuffer");
            Previous = creator.CreateTexture2D("Previous", new Texture2DDescription(Format.R16G16B16A16Float, width, height, 1, 1), false);

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

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
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

            context.SetGraphicsPipeline(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

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
            creator.ReleaseResource("Previous");
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}