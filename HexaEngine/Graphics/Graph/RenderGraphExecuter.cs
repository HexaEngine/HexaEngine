namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Profiling;

    public class RenderGraphExecuter
    {
        private readonly GraphResourceBuilder resourceCreator;
        private readonly GraphPipelineBuilder pipelineCreator;
        private readonly RenderGraph renderGraph;
        private readonly HashSet<RenderGraphNode> triggeredPasses = [];
        private bool oneHitPassed;

        public RenderGraphExecuter(IGraphicsDevice device, RenderGraph renderGraph)
        {
            this.renderGraph = renderGraph;
            resourceCreator = new GraphResourceBuilder(device);
            pipelineCreator = new GraphPipelineBuilder(device);
        }

        public GraphResourceBuilder ResourceBuilder => resourceCreator;

        public void Init(ICPUProfiler? profiler)
        {
            foreach (var node in renderGraph.SortedNodes)
            {
                resourceCreator.Container = node.Container;
                node.Pass.Init(resourceCreator, profiler);
                resourceCreator.Container = null;
            }

            resourceCreator.CreateResources();

            foreach (var node in renderGraph.SortedNodes)
            {
                node.Pass.Prepare(resourceCreator);
            }
        }

        public void TriggerOneHit()
        {
            oneHitPassed = false;
        }

        public void TriggerPass(string passName)
        {
            var node = renderGraph.GetNodeByName(passName);
            if (node != null)
            {
                triggeredPasses.Add(node);
            }
        }

        [Profile]
        public void Execute(IGraphicsContext context, ICPUProfiler? profiler)
        {
            for (int i = 0; i < renderGraph.SortedNodes.Count; i++)
            {
                var node = renderGraph.SortedNodes[i];
                var pass = node.Pass;
                if (pass.Type == RenderPassType.OneHit && oneHitPassed)
                {
                    continue;
                }
                if (pass.Type == RenderPassType.Trigger && !triggeredPasses.Contains(node))
                {
                    continue;
                }
                else if (pass.Type == RenderPassType.Trigger)
                {
                    triggeredPasses.Remove(node);
                }

#if DEBUG
                context.BeginEvent(pass.Name);
#endif
                profiler?.Begin(pass.Name);
                pass.Execute(context, resourceCreator, profiler);
                profiler?.End(pass.Name);
#if DEBUG
                context.EndEvent();
#endif
            }
            oneHitPassed = true;
        }

        public void ResizeBegin()
        {
            resourceCreator.ReleaseResources();
            pipelineCreator.ReleaseResources();
            for (int i = 0; i < renderGraph.SortedNodes.Count; i++)
            {
                var node = renderGraph.SortedNodes[i];
                node.Pass.OnResize(resourceCreator);
            }
        }

        public void ResizeEnd(ICPUProfiler? profiler)
        {
            for (int i = 0; i < renderGraph.SortedNodes.Count; i++)
            {
                var node = renderGraph.SortedNodes[i];
                node.Pass.Init(resourceCreator, profiler);
            }
            resourceCreator.CreateResources();
        }

        public void Release()
        {
            resourceCreator.ReleaseResources();
            pipelineCreator.ReleaseResources();
            for (int i = 0; i < renderGraph.SortedNodes.Count; i++)
            {
                var node = renderGraph.SortedNodes[i];
                node.Pass.Release();
            }
        }
    }
}