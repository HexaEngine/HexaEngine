namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;
    using System.Runtime.CompilerServices;

    public class GraphPipelineBuilder
    {
        private readonly IGraphicsDevice device;
        private readonly List<IGraphicsPipeline> graphicsPipelines = new();
        private readonly List<IComputePipeline> computePipelines = new();

        public GraphPipelineBuilder(IGraphicsDevice device)
        {
            this.device = device;
        }

        internal void CreateResources()
        {
        }

        internal void ReleaseResources()
        {
            for (int i = 0; i < graphicsPipelines.Count; i++)
            {
                graphicsPipelines[i].Dispose();
            }
            graphicsPipelines.Clear();

            for (int i = 0; i < computePipelines.Count; i++)
            {
                computePipelines[i].Dispose();
            }
            computePipelines.Clear();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, bool lazyInit = true, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, bool lazyInit = true, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateComputePipeline(desc, filename, line);
            computePipelines.Add(pipeline);
            return pipeline;
        }
    }
}