namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes.Managers;

    public class ObjectCullPass : RenderPass<ObjectCullPass>
    {
        private ResourceRef<DepthMipChain> chain = null!;

        public override void BuildDependencies(GraphDependencyBuilder builder)
        {
            builder.RunAfter<HizDepthPass>();
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            chain = creator.GetDepthMipChain("HiZBuffer");
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var camera = CameraManager.Culling!;
            var manager = CullingManager.Current;
            var cull = manager.Context;

            manager.UpdateCamera(context, camera, creator.Viewport);
            manager.ExecuteCulling(context, camera, cull.Count, cull.TypeCount, chain.Value!, profiler);
        }
    }
}