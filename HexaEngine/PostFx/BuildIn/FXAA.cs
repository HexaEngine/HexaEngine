#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

    public class FXAA : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;

        public override string Name => "FXAA";

        public override PostFxFlags Flags => PostFxFlags.None;

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            builder.RunAfter("Compose");
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public override void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public override void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
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

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            sampler.Dispose();
        }
    }
}