namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;

    public class GraphicsPipelineResourceFactory : ResourceFactory<ResourceInstance<IGraphicsPipeline>, (GraphicsPipelineDesc, GraphicsPipelineState, ShaderMacro[])>
    {
        private readonly IGraphicsDevice device;

        public GraphicsPipelineResourceFactory(ResourceManager resourceManager, IGraphicsDevice device) : base(resourceManager)
        {
            this.device = device;
        }

        protected override ResourceInstance<IGraphicsPipeline> CreateInstance(ResourceManager manager, string name, (GraphicsPipelineDesc, GraphicsPipelineState, ShaderMacro[]) instanceData)
        {
            return new ResourceInstance<IGraphicsPipeline>(this, name);
        }

        protected override void LoadInstance(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance, (GraphicsPipelineDesc, GraphicsPipelineState, ShaderMacro[]) instanceData)
        {
            (GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros) = instanceData;
            var pipeline = device.CreateGraphicsPipeline(desc, state, macros);
            instance.EndLoad(pipeline);
        }

        protected override async Task LoadInstanceAsync(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance, (GraphicsPipelineDesc, GraphicsPipelineState, ShaderMacro[]) instanceData)
        {
            (GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros) = instanceData;
            var pipeline = await device.CreateGraphicsPipelineAsync(desc, state, macros);
            instance.EndLoad(pipeline);
        }

        protected override void UnloadInstance(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance)
        {
        }
    }
}