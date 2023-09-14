﻿namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graph;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;

    public class ObjectCullPass : ComputePass
    {
        private bool isEnabled;
        private ResourceRef<DepthMipChain> chain;

        public ObjectCullPass() : base("ObjectCull")
        {
            AddReadDependency(new("HiZBuffer"));
        }

        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            chain = creator.GetDepthMipChain("HiZBuffer");
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            if (!isEnabled)
                return;

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            renderers.VisibilityTest(context, creator.Viewport, chain.Value.SRV, RenderQueueIndex.Geometry);
        }
    }
}