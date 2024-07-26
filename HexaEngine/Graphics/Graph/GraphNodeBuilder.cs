namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Profiling;

    public class GraphNodeBuilder
    {
        private GraphDependencyBuilder referenceBuilder;
        private GraphResourceBuilder resourceBuilder;
        private GraphPipelineBuilder pipelineBuilder;
        private readonly RenderGraphNode node;

        public GraphNodeBuilder(GraphResourceBuilder resourceBuilder, GraphPipelineBuilder pipelineBuilder, RenderGraphNode node)
        {
            this.resourceBuilder = resourceBuilder;
            this.pipelineBuilder = pipelineBuilder;
            this.node = node;
            referenceBuilder = new(node);
        }

        public void Build(RenderPass pass, RenderGraph graph, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            pass.Init(resourceBuilder, profiler);
            referenceBuilder.Build(graph);
        }
    }
}