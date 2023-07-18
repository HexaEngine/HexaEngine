namespace HexaEngine.Rendering.Graph
{
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

        public void Init()
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                renderPasses[renderGraph.SortedNodeIndices[i]].Init(resourceCreator, pipelineCreator, device);
            }
            resourceCreator.CreateResources();
        }

        public void Execute(IGraphicsContext context)
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                renderPasses[renderGraph.SortedNodeIndices[i]].Execute(context, resourceCreator);
            }
        }

        public void Release()
        {
            resourceCreator.ReleaseResources();
            pipelineCreator.ReleaseResources();
        }
    }
}