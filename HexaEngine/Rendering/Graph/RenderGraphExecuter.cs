namespace HexaEngine.Rendering.Graph
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
        private HashSet<RenderPass> triggeredPasses;
        private bool oneHitPassed;

        public RenderGraphExecuter(IGraphicsDevice device, RenderGraph renderGraph, RenderPass[] renderPasses)
        {
            this.device = device;
            this.renderGraph = renderGraph;
            this.renderPasses = renderPasses;
            resourceCreator = new GraphResourceBuilder(device);
            pipelineCreator = new GraphPipelineBuilder(device);
        }

        public GraphResourceBuilder ResourceBuilder => resourceCreator;

        public void Init(ICPUProfiler? profiler)
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                var pass = renderPasses[renderGraph.SortedNodeIndices[i]];
                pass.Init(resourceCreator, pipelineCreator, device, profiler);
                profiler?.CreateStage(pass.Name);
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

                profiler?.Begin(pass.Name);
                pass.Execute(context, resourceCreator, profiler);
                profiler?.End(pass.Name);
            }
            oneHitPassed = true;
        }

        public void Release()
        {
            resourceCreator.ReleaseResources();
            pipelineCreator.ReleaseResources();
        }
    }
}