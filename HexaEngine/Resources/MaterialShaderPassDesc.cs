namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public struct MaterialShaderPassDesc
    {
        public string Name;
        public GraphicsPipelineDescEx Pipeline;
        public GraphicsPipelineStateDesc State;

        public MaterialShaderPassDesc(string name, GraphicsPipelineDescEx pipeline, GraphicsPipelineStateDesc state)
        {
            Name = name;
            Pipeline = pipeline;
            State = state;
        }
    }
}