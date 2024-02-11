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

        protected override Task LoadInstanceAsync(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance, GraphicsPipelineDesc desc)
        {
            var pipeline = device.CreateGraphicsPipeline(desc);
            instance.EndLoad(pipeline);
            return Task.CompletedTask;
        }

        protected override void UnloadInstance(ResourceManager manager, ResourceInstance<IGraphicsPipeline> instance)
        {
        }
    }
}