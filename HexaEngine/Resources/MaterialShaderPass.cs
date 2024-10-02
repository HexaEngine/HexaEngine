namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public class MaterialShaderPass : IDisposable
    {
        private readonly string name;
        private readonly IGraphicsPipelineState pipelineState;

        public MaterialShaderPass(string name, IGraphicsDevice device, IGraphicsPipeline pipeline, GraphicsPipelineStateDesc desc)
        {
            this.name = name;
            pipelineState = device.CreateGraphicsPipelineState(pipeline, desc);
        }

        public MaterialShaderPass(string name, IGraphicsDevice device, GraphicsPipelineDescEx pipelineDesc, GraphicsPipelineStateDesc desc)
        {
            this.name = name;
            pipelineState = device.CreateGraphicsPipelineState(pipelineDesc, desc);
        }

        public string Name => name;

        public IResourceBindingList Bindings => pipelineState.Bindings;

        public bool BeginDraw(IGraphicsContext context)
        {
            if (!pipelineState.IsValid)
            {
                return false;
            }

            context.SetGraphicsPipelineState(pipelineState);
            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            pipelineState.Dispose();
        }

        public override string ToString()
        {
            return name;
        }
    }
}