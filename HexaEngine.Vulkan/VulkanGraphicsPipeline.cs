namespace HexaEngine.Vulkan
{
    using HexaEngine.Core.Graphics;

    public class VulkanGraphicsPipeline : IGraphicsPipeline
    {
        private readonly GraphicsPipelineDesc desc;

        public GraphicsPipelineDesc Description { get; }

        public string DebugName { get; }

        public ShaderMacro[]? Macros { get; set; }

        public bool IsInitialized { get; }

        public bool IsValid { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Recompile()
        {
            throw new NotImplementedException();
        }

        private void Compile()
        {
        }
    }
}