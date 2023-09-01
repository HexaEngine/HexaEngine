namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graph;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;

    public class DepthPrePass : RenderPass
    {
        private ResourceRef<DepthStencil> depthStencil;

        public DepthPrePass() : base("PrePass")
        {
            AddWriteDependency(new("#DepthStencil"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var depthStencilBuffer = depthStencil.Value;
            context.ClearDepthStencilView(depthStencilBuffer.DSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.SetRenderTarget(null, depthStencilBuffer.DSV);
            context.SetViewport(depthStencilBuffer.Viewport);

            profiler?.Begin("PrePass.Geometry");
            var geometry = renderers.GeometryQueue;
            for (int i = 0; i < geometry.Count; i++)
            {
                var renderer = geometry[i];
                profiler?.Begin($"PrePass.{renderer.DebugName}");
                renderer.DrawDepth(context);
                profiler?.End($"PrePass.{renderer.DebugName}");
            }
            profiler?.End("PrePass.Geometry");

            profiler?.Begin("PrePass.Transparency");
            var transparency = renderers.TransparencyQueue;
            for (int i = 0; i < transparency.Count; i++)
            {
                var renderer = transparency[i];
                profiler?.Begin($"PrePass.{renderer.DebugName}");
                renderer.DrawDepth(context);
                profiler?.End($"PrePass.{renderer.DebugName}");
            }
            profiler?.End("PrePass.Transparency");

            context.ClearState();
        }
    }
}