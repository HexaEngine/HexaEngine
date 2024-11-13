namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;

    [EditorDisplayName("Fog")]
    public class Fog : PostFxBase
    {
#nullable disable

        private IGraphicsPipelineState fog;

#nullable restore

        public override string Name { get; } = "Fog";

        public override PostFxFlags Flags { get; }

        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunAfter<HBAO>()
                .RunAfter<SSGI>()
                .RunAfter<SSR>()
                .RunBefore<MotionBlur>()
                .RunBefore<AutoExposure>()
                .RunBefore<TAA>()
                .RunBefore<DepthOfField>()
                .RunBefore<ChromaticAberration>()
                .RunBefore<Bloom>()
                .RunAfter<VolumetricLighting>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            fog = device.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    PixelShader = "effects/fog/ps.hlsl",
                    VertexShader = "quad.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultFullscreen
            });
        }

        public override void UpdateBindings()
        {
            fog.Bindings.SetSRV("hdrTexture", Input);
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(fog);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            fog.Dispose();
        }
    }
}