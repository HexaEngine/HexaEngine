#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

    /// <summary>
    /// Post-processing effect for Fast Approximate Anti-Aliasing (FXAA).
    /// </summary>
    public class FXAA : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        /// <inheritdoc/>
        public override string Name => "FXAA";

        /// <inheritdoc/>
        public override PostFxFlags Flags => PostFxFlags.None;

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                    .RunAfter("ColorGrading")
                    .RunAfter("Grain")
                    .RunAfter("UserLUT");
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipeline.Dispose();
            sampler.Dispose();
        }
    }
}