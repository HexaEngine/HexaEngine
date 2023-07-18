namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Core.Resources;
    using HexaEngine.PostFx;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Rendering.Graph;

    public class TAA : IPostFx, IAntialiasing
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
        private int priority = 400;
        private bool enabled = true;

        private float alpha = 0.1f;
        private float colorBoxSigma = 1f;
        private bool antiFlicker = true;
        private bool dirty = true;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "TAA";

        public PostFxFlags Flags => PostFxFlags.None;

        public bool Enabled
        {
            get => enabled; set
            {
                enabled = value;
                OnEnabledChanged?.Invoke(value);
            }
        }

        public int Priority
        {
            get => priority; set
            {
                priority = value;
                OnPriorityChanged?.Invoke(value);
            }
        }

        public float Alpha
        {
            get => alpha; set
            {
                alpha = value;
                dirty = true;
            }
        }

        public float ColorBoxSigma
        {
            get => colorBoxSigma; set
            {
                colorBoxSigma = value;
                dirty = true;
            }
        }

        public bool AntiFlicker
        {
            get => antiFlicker; set
            {
                antiFlicker = value;
                dirty = true;
            }
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

        public async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
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

        public void Resize(int width, int height)
        {
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
            OutputTex = resource;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(alpha, colorBoxSigma, antiFlicker));
                dirty = false;
            }
        }

        public unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
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

        public void Dispose()
        {
            pipeline.Dispose();
            sampler.Dispose();
            paramsBuffer.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}