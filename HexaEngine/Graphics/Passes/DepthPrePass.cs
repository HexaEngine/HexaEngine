namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;

    public class DepthPrePass : RenderPass
    {
        private ResourceRef<DepthStencil> depthStencil;

        public DepthPrePass() : base("PrePass")
        {
            AddWriteDependency(new("#DepthStencil"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");

            // force init.
            _ = CullingManager.Current;
        }

        public override void Release()
        {
            CullingManager.Current.Release();
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var culling = CullingManager.Current;

            culling.Context.Reset();

            profiler?.Begin("VisibilityTest.Geometry");
            RenderManager.ExecuteGroupVisibilityTest(renderers.GeometryQueue, CullingManager.Current.Context, profiler, "Geometry");
            profiler?.End("VisibilityTest.Geometry");

            profiler?.Begin("VisibilityTest.AlphaTest");
            RenderManager.ExecuteGroupVisibilityTest(renderers.AlphaTestQueue, CullingManager.Current.Context, profiler, "AlphaTest");
            profiler?.End("VisibilityTest.AlphaTest");

            profiler?.Begin("VisibilityTest.Transparency");
            RenderManager.ExecuteGroupVisibilityTest(renderers.TransparencyQueue, CullingManager.Current.Context, profiler, "Transparency");
            profiler?.End("VisibilityTest.Transparency");

            profiler?.Begin("VisibilityTest.GeometryLast");
            RenderManager.ExecuteGroupVisibilityTest(renderers.GeometryLastQueue, CullingManager.Current.Context, profiler, "GeometryLast");
            profiler?.End("VisibilityTest.GeometryLast");

            profiler?.Begin("VisibilityTest.Update");
            culling.Context.Flush(context);
            profiler?.End("VisibilityTest.Update");

            var depthStencilBuffer = depthStencil.Value;
            context.ClearDepthStencilView(depthStencilBuffer.DSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.SetRenderTarget(null, depthStencilBuffer.DSV);

            context.SetViewport(depthStencilBuffer.Viewport);

            profiler?.Begin("PrePass.Geometry");
            RenderManager.ExecuteGroupDepth(renderers.GeometryQueue, context, profiler, "Geometry");
            profiler?.End("PrePass.Geometry");

            profiler?.Begin("PrePass.AlphaTest");
            RenderManager.ExecuteGroupDepth(renderers.AlphaTestQueue, context, profiler, "AlphaTest");
            profiler?.End("PrePass.AlphaTest");

            profiler?.Begin("PrePass.GeometryLast");
            RenderManager.ExecuteGroupDepth(renderers.GeometryLastQueue, context, profiler, "GeometryLast");
            profiler?.End("PrePass.GeometryLast");

            context.ClearState();
        }
    }
}