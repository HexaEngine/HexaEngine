#nullable disable


namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Mathematics;

    public class FXAA : IPostFx
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;

        public string Name => "FXAA";

        public PostFxFlags Flags => PostFxFlags.None;

        public bool Enabled { get; set; } = true;

        public int Priority { get; set; } = -1;

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            quad = new Quad(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/fxaa/vs.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl"
            }, macros);
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
        }

        public void Resize(int width, int height)
        {
        }

        public void SetOutput(IRenderTargetView view, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public void SetInput(IShaderResourceView view)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(Input, 0);
            context.PSSetSampler(sampler, 0);
            quad.DrawAuto(context, pipeline);
            context.ClearState();
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