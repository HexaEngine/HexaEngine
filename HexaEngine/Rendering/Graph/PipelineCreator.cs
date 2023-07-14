namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Graphics;
    using System.Runtime.CompilerServices;

    public class PipelineCreator
    {
        private readonly IGraphicsDevice device;
        private readonly List<IGraphicsPipeline> graphicsPipelines = new();
        private readonly List<IComputePipeline> computePipelines = new();

        public PipelineCreator(IGraphicsDevice device)
        {
            this.device = device;
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

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, macros, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, elementDescriptions, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, inputElements, macros, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, state, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, state, macros, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, state, elementDescriptions, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateGraphicsPipeline(desc, state, inputElements, macros, filename, line);
            graphicsPipelines.Add(pipeline);
            return pipeline;
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateComputePipeline(desc, filename, line);
            computePipelines.Add(pipeline);
            return pipeline;
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            var pipeline = device.CreateComputePipeline(desc, macros, filename, line);
            computePipelines.Add(pipeline);
            return pipeline;
        }
    }
}