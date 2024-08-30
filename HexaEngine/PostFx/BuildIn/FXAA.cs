#nullable disable

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.PostFx;

    /// <summary>
    /// Post-processing effect for Fast Approximate Anti-Aliasing (FXAA).
    /// </summary>
    [EditorDisplayName("FXAA")]
    public class FXAA : PostFxBase
    {
        private IGraphicsPipelineState pipeline;
        private ISamplerState sampler;

        /// <inheritdoc/>
        public override string Name => "FXAA";

        /// <inheritdoc/>
        public override PostFxFlags Flags => PostFxFlags.None;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.SDR;

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                    .RunAfter<ColorGrading>()
                    .RunAfter<Grain>()
                    .RunAfter<UserLUT>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("g_txProcessed", Input);
            pipeline.Bindings.SetSampler("g_samLinear", sampler);
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
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