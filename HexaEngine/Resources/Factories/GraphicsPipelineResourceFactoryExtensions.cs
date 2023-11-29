namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;

    public static class GraphicsPipelineResourceFactoryExtensions
    {
        public static ResourceInstance<IGraphicsPipeline>? LoadPipeline(this ResourceManager manager, string name, GraphicsPipelineDesc desc)
        {
            return manager.CreateInstance<ResourceInstance<IGraphicsPipeline>, GraphicsPipelineDesc>(name, desc);
        }

        public static async Task<ResourceInstance<IGraphicsPipeline>?> LoadPipelineAsync(this ResourceManager manager, string name, GraphicsPipelineDesc desc)
        {
            return await manager.CreateInstanceAsync<ResourceInstance<IGraphicsPipeline>, GraphicsPipelineDesc>(name, desc);
        }

        public static void UpdatePipeline(this ResourceManager manager, ResourceInstance<IGraphicsPipeline>? pipeline, GraphicsPipelineDesc desc)
        {
            if (pipeline == null)
            {
                return;
            }
            var pipe = manager.GraphicsDevice.CreateGraphicsPipeline(desc);
            pipeline.BeginLoad();
            pipeline.EndLoad(pipe);
        }

        public static async void UpdatePipelineAsync(this ResourceManager manager, ResourceInstance<IGraphicsPipeline>? pipeline, GraphicsPipelineDesc desc)
        {
            if (pipeline == null)
            {
                return;
            }
            var pipe = await manager.GraphicsDevice.CreateGraphicsPipelineAsync(desc);
            pipeline.BeginLoad();
            pipeline.EndLoad(pipe);
        }
    }
}