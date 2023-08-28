namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

    public class TAA : PostFxBase, IAntialiasing
    {
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        private ConstantBuffer<TAAParams> paramsBuffer;

        private ResourceRef<Texture2D> Velocity;
        private ResourceRef<Texture2D> Previous;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public ITexture2D OutputTex;
        public Viewport Viewport;

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

        public override async Task InitializeAsync(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .AddBinding("VelocityBuffer")
                .RunBefore("Compose")
                .RunBefore("HBAO")
                .RunBefore("DepthOfField")
                .RunBefore("GodRays")
                .RunBefore("VolumetricClouds")
                .RunBefore("SSR")
                .RunBefore("SSGI")
                .RunBefore("LensFlare")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/taa/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);

            paramsBuffer = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = ResourceManager2.Shared.GetTexture("VelocityBuffer");
            Previous = ResourceManager2.Shared.AddTexture("Previous", new Texture2DDescription(Format.R16G16B16A16Float, width, height, 1, 1));

            Viewport = new(width, height);
        }

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .AddBinding("VelocityBuffer")
                .RunBefore("Compose")
                .RunBefore("HBAO")
                .RunBefore("DepthOfField")
                .RunBefore("GodRays")
                .RunBefore("VolumetricClouds")
                .RunBefore("SSR")
                .RunBefore("SSGI")
                .RunBefore("LensFlare")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/taa/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);

            paramsBuffer = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = ResourceManager2.Shared.GetTexture("VelocityBuffer");
            Previous = ResourceManager2.Shared.AddTexture("Previous", new Texture2DDescription(Format.R16G16B16A16Float, width, height, 1, 1));

            Viewport = new(width, height);
        }

        public override void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
            OutputTex = resource;
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
            context.CopyResource(Previous.Value, OutputTex);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            sampler.Dispose();
            paramsBuffer.Dispose();
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}