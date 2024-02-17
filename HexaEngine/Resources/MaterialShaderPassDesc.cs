namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public struct MaterialShaderPassDesc
    {
        public string Name;
        public GraphicsPipelineDesc Pipeline;
        public GraphicsPipelineStateDesc State;

        public MaterialShaderPassDesc(string name, GraphicsPipelineDesc pipeline, GraphicsPipelineStateDesc state)
        {
            Name = name;
            Pipeline = pipeline;
            State = state;
        }
    }
}