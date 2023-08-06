namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;

    public class RenderGraphExecuter
    {
        private IGraphicsDevice device;
        private GraphResourceBuilder resourceCreator;
        private GraphPipelineBuilder pipelineCreator;
        private RenderGraph renderGraph;
        private RenderPass[] renderPasses;

        public RenderGraphExecuter(IGraphicsDevice device, RenderGraph renderGraph, RenderPass[] renderPasses)
        {
            this.device = device;
            this.renderGraph = renderGraph;
            this.renderPasses = renderPasses;
            resourceCreator = new GraphResourceBuilder(device);
            pipelineCreator = new GraphPipelineBuilder(device);
        }

        public GraphResourceBuilder ResourceCreator => resourceCreator;

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

        public void Execute(IGraphicsContext context, ICPUProfiler? profiler)
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                var pass = renderPasses[renderGraph.SortedNodeIndices[i]];
                profiler?.Begin(pass.Name);
                pass.Execute(context, resourceCreator, profiler);
                profiler?.End(pass.Name);
            }
        }

        public void Release()
        {
            resourceCreator.ReleaseResources();
            pipelineCreator.ReleaseResources();
        }
    }
}