namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;

    public class OverlayPass : RenderPass
    {
        private ResourceRef<IGraphicsPipeline> copyPipeline;
        private ResourceRef<ISamplerState> samplerState;

        private ResourceRef<DepthStencil> depthStencil;
        private ResourceRef<Texture2D> postFxBuffer;

        private DebugDrawRenderer debugDrawRenderer;

        public OverlayPass() : base("OverlayPass")
        {
            AddReadDependency(new("#DepthStencil"));
            AddReadDependency(new("PostFxBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            copyPipeline = creator.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = [new ShaderMacro("SAMPLED", 1)]
            });
            samplerState = creator.CreateSamplerState("LinearClamp", SamplerStateDescription.LinearClamp);

            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            postFxBuffer = creator.GetTexture2D("PostFxBuffer");

            debugDrawRenderer = new(creator.Device);
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            if (Application.InEditorMode)
            {
                profiler?.Begin("DebugDraw");
                context.SetRenderTarget(postFxBuffer.Value, depthStencil.Value);
                DebugDraw.SetViewport(creator.Viewport);
                debugDrawRenderer?.EndDraw();
                debugDrawRenderer?.BeginDraw();
                profiler?.End("DebugDraw");
            }

            context.SetRenderTarget(creator.Output, null);
            context.SetViewport(creator.OutputViewport);
            context.SetGraphicsPipeline(copyPipeline.Value);
            context.PSSetShaderResource(0, postFxBuffer.Value);
            context.PSSetSampler(0, samplerState.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);
            context.SetGraphicsPipeline(null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        public override void Release()
        {
            debugDrawRenderer.Dispose();
        }
    }
}