using HexaEngine.Core.Graphics;
using HexaEngine.Graphics.Graph;

namespace HexaEngine.PostFx.BuildIn
{
    public class LinearDepth : PostFxBase
    {
        private ResourceRef<Texture2D> buffer;
        private ResourceRef<IGraphicsPipelineState> pso;

        public override string Name { get; } = "LinearDepth";

        public override PostFxFlags Flags { get; } = PostFxFlags.NoInput | PostFxFlags.NoOutput | PostFxFlags.Optional;

        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.None;

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            buffer = creator.CreateBuffer("LinearDepth", Format.R32Float, 1, 1, GpuAccessFlags.All, CpuAccessFlags.None, creationFlags: ResourceCreationFlags.LazyInit);

            pso = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    PixelShader = "HexaEngine.Core:shaders/effects/depth/ps.hlsl",
                    VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultFullscreen
            });
        }

        public override void UpdateBindings()
        {
            pso.Value!.Bindings.SetSRV("sourceTex", buffer.Value);
        }

        public override void Draw(IGraphicsContext context)
        {
            context.Device.SetGlobalSRV("linearDepthTex", buffer.Value);
            context.SetRenderTarget(buffer.Value!.RTV, null);
            context.SetGraphicsPipelineState(pso.Value!);
            context.SetViewport(Viewport);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
        }
    }
}