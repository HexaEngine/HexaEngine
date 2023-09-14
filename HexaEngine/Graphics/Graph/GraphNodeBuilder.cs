﻿namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;

    public class GraphNodeBuilder
    {
        private GraphReferenceBuilder referenceBuilder;
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
            pass.Init(resourceBuilder, pipelineBuilder, device, profiler);
            referenceBuilder.Build(graph);
        }
    }
}