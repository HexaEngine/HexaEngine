namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System.Threading.Tasks;

    public class MotionBlur : IPostFx
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        private ResourceRef<Texture> Velocity;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;
        private bool enabled = true;
        private int priority = 300;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "MotionBlur";

        public PostFxFlags Flags { get; }

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
                VertexShader = "effects/motionblur/vs.hlsl",
                PixelShader = "effects/motionblur/ps.hlsl"
            }, macros);
            sampler = device.CreateSamplerState(SamplerDescription.LinearWrap);

            Velocity = ResourceManager2.Shared.GetTexture("VelocityBuffer");

            Viewport = new(width, height);
        }

        public void Resize(int width, int height)
        {
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
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

            nint* srvs = stackalloc nint[2];
            srvs[0] = Input.NativePointer;
            srvs[1] = Velocity.Value.ShaderResourceView.NativePointer;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetSampler(0, sampler);
            quad.DrawAuto(context, pipeline);
            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
        }
    }
}