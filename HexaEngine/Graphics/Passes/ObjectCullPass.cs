namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes.Managers;

    public class ObjectCullPass : ComputePass
    {
        private bool isEnabled;
        private ResourceRef<DepthMipChain> chain;

        public ObjectCullPass() : base("ObjectCull")
        {
            AddReadDependency(new("HiZBuffer"));
        }

        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            chain = creator.GetDepthMipChain("HiZBuffer");
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var camera = CameraManager.Culling;
            var manager = CullingManager.Current;
            var cull = manager.Context;

            manager.UpdateCamera(context, camera, creator.Viewport);
            manager.ExecuteCulling(context, camera, cull.Count, cull.TypeCount, chain.Value, profiler);
        }
    }
}