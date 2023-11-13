namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Graph;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Rendering.Renderers;

    public class OverlayPass : RenderPass
    {
        private IGraphicsPipeline copyPipeline;
        private ResourceRef<ISamplerState> samplerState;

        private ResourceRef<DepthStencil> depthStencil;
        private ResourceRef<Texture2D> postFxBuffer;

        private DebugDrawRenderer debugDrawRenderer;

        public OverlayPass() : base("OverlayPass")
        {
            AddReadDependency(new("#DepthStencil"));
            AddReadDependency(new("PostFxBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            copyPipeline = pipelineCreator.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl"
            },
            GraphicsPipelineState.DefaultFullscreen, [new ShaderMacro("SAMPLED", 1)]);
            samplerState = creator.CreateSamplerState("LinearClamp", SamplerStateDescription.LinearClamp);

            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            postFxBuffer = creator.GetTexture2D("PostFxBuffer");

            debugDrawRenderer = new(device);
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
            context.SetGraphicsPipeline(copyPipeline);
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
            copyPipeline.Dispose();

            debugDrawRenderer.Dispose();
        }
    }
}