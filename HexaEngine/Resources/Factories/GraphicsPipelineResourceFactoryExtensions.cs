namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;

    public static class GraphicsPipelineResourceFactoryExtensions
    {
        public static ResourceInstance<IGraphicsPipeline>? LoadPipeline(this ResourceManager1 manager, string name, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            return manager.CreateInstance<ResourceInstance<IGraphicsPipeline>, (GraphicsPipelineDesc, GraphicsPipelineState, ShaderMacro[])>(name, (desc, state, macros));
        }

        public static async Task<ResourceInstance<IGraphicsPipeline>?> LoadPipelineAsync(this ResourceManager1 manager, string name, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            return await manager.CreateInstanceAsync<ResourceInstance<IGraphicsPipeline>, (GraphicsPipelineDesc, GraphicsPipelineState, ShaderMacro[])>(name, (desc, state, macros));
        }

        public static void UpdatePipeline(this ResourceManager1 manager, ResourceInstance<IGraphicsPipeline>? pipeline, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            if (pipeline == null)
            {
                return;
            }
            var pipe = manager.GraphicsDevice.CreateGraphicsPipeline(desc, state);
            pipeline.BeginLoad();
            pipeline.EndLoad(pipe);
        }

        public static async void UpdatePipelineAsync(this ResourceManager1 manager, ResourceInstance<IGraphicsPipeline>? pipeline, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            if (pipeline == null)
            {
                return;
            }
            var pipe = await manager.GraphicsDevice.CreateGraphicsPipelineAsync(desc, state);
            pipeline.BeginLoad();
            pipeline.EndLoad(pipe);
        }

        public static void UnloadPipeline(this ResourceManager1 manager, ResourceInstance<IGraphicsPipeline>? pipeline)
        {
            manager.DestroyInstance(pipeline);
        }
    }
}