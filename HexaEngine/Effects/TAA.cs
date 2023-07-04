namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Mathematics;
    using HexaEngine.Core.Resources;

    public class TAA : IPostFx, IAntialiasing
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        private ResourceRef<Texture> Velocity;
        private ResourceRef<Texture> Previous;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public ITexture2D InputTex;
        public Viewport Viewport;
        private int priority = 2;
        private bool enabled = true;

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

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            quad = new Quad(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/taa/vs.hlsl",
                PixelShader = "effects/taa/ps.hlsl"
            }, macros);
            sampler = device.CreateSamplerState(SamplerDescription.LinearWrap);

            Velocity = ResourceManager2.Shared.GetTexture("VelocityBuffer");
            Previous = ResourceManager2.Shared.AddTexture("Previous", TextureDescription.CreateTexture2D(width, height, 1, Format.R16G16B16A16Float));

            Viewport = new(width, height);
        }

        public void Resize(int width, int height)
        {
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
            InputTex = resource;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            nint* srvs = stackalloc nint[3];
            srvs[0] = Input.NativePointer;
            srvs[1] = Previous.Value.ShaderResourceView.NativePointer;
            srvs[2] = Velocity.Value.ShaderResourceView.NativePointer;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.PSSetSampler(0, sampler);
            quad.DrawAuto(context, pipeline);
            context.PSSetSampler(0, null);
            ZeroMemory(srvs, sizeof(nint) * 3);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.SetRenderTarget(null, default);
            context.CopyResource(Previous.Value.Resource, InputTex);
        }

        public void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}