#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

    public class FXAA : IPostFx, IAntialiasing
    {
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;
        private bool enabled = false;
        private int priority = -1;

        public event Action<bool> OnEnabledChanged;

        public event Action<int> OnPriorityChanged;

        public string Name => "FXAA";

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

        public async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder.RunAfter("Compose");
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            sampler = device.CreateSamplerState(SamplerStateDescription.AnisotropicClamp);
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

        public void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
            {
                return;
            }

            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetSampler(0, sampler);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);
            context.SetRenderTarget(null, null);
        }

        public void Dispose()
        {
            pipeline.Dispose();
            sampler.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}