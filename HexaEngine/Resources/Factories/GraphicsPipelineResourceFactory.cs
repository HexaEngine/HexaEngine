namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;

    public class GraphicsPipelineResourceFactory : ResourceFactory<ResourceInstance<IGraphicsPipeline>, GraphicsPipelineDesc>
    {
        private readonly IGraphicsDevice device;

        public GraphicsPipelineResourceFactory(ResourceManager resourceManager, IGraphicsDevice device) : base(resourceManager)
        {
            this.device = device;
        }

        protected override ResourceInstance<IGraphicsPipeline> CreateInstance(ResourceManager manager, string name, GraphicsPipelineDesc insdesctanceData)
        {
            return new ResourceInstance<IGraphicsPipeline>(this, name);
        }

        protected override void LoadInstance(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance, GraphicsPipelineDesc desc)
        {
            var pipeline = device.CreateGraphicsPipeline(desc);
            instance.EndLoad(pipeline);
        }

        protected override async Task LoadInstanceAsync(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance, GraphicsPipelineDesc desc)
        {
            var pipeline = await device.CreateGraphicsPipelineAsync(desc);
            instance.EndLoad(pipeline);
        }

        protected override void UnloadInstance(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance)
        {
        }
    }
}