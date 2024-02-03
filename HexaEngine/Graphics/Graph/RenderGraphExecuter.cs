namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;

    public class RenderGraphExecuter
    {
        private readonly IGraphicsDevice device;
        private readonly GraphResourceBuilder resourceCreator;
        private readonly GraphPipelineBuilder pipelineCreator;
        private readonly RenderGraph renderGraph;
        private readonly RenderPass[] renderPasses;
        private readonly RenderPass[] renderPassesSorted;
        private HashSet<RenderPass> triggeredPasses = new();
        private bool oneHitPassed;

        public RenderGraphExecuter(IGraphicsDevice device, RenderGraph renderGraph, RenderPass[] renderPasses)
        {
            this.device = device;
            this.renderGraph = renderGraph;
            this.renderPasses = renderPasses;
            renderPassesSorted = new RenderPass[renderPasses.Length];
            resourceCreator = new GraphResourceBuilder(device);
            pipelineCreator = new GraphPipelineBuilder(device);
        }

        public GraphResourceBuilder ResourceBuilder => resourceCreator;

        public IReadOnlyList<RenderPass> RenderPasses => renderPasses;

        public IReadOnlyList<RenderPass> RenderPassesSorted => renderPassesSorted;

        public void Init(ICPUProfiler? profiler)
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                var idx = renderGraph.SortedNodeIndices[i];
                var pass = renderPasses[idx];
                renderPassesSorted[idx] = pass;
                pass.Init(resourceCreator, profiler);
            }
            resourceCreator.CreateResources();
        }

        public void TriggerOneHit()
        {
            oneHitPassed = false;
        }

        public void TriggerPass(string passName)
        {
            for (int i = 0; i < renderPasses.Length; i++)
            {
                var pass = renderPasses[i];
                if (pass.Name == passName)
                {
                    triggeredPasses.Add(pass);
                    break;
                }
            }
        }

        public void Execute(IGraphicsContext context, ICPUProfiler? profiler)
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                var pass = renderPasses[renderGraph.SortedNodeIndices[i]];
                if (pass.Type == RenderPassType.OneHit && oneHitPassed)
                {
                    continue;
                }
                if (pass.Type == RenderPassType.Trigger && !triggeredPasses.Contains(pass))
                {
                    continue;
                }
                else if (pass.Type == RenderPassType.Trigger)
                {
                    triggeredPasses.Remove(pass);
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

        public void Release()
        {
            resourceCreator.ReleaseResources();
            pipelineCreator.ReleaseResources();
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                var pass = renderPasses[renderGraph.SortedNodeIndices[i]];
                pass.Release();
            }
        }
    }
}