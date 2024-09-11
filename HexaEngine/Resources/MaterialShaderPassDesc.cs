namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public struct MaterialShaderPassDesc
    {
        public string Name;
        public GraphicsPipelineDescEx Pipeline;
        public GraphicsPipelineStateDesc State;
        public bool SurfaceShader;
        public string? BaseShader;

        public MaterialShaderPassDesc(string name, GraphicsPipelineDescEx pipeline, GraphicsPipelineStateDesc state, bool surfaceShader, string? baseShader = null)
        {
            Name = name;
            Pipeline = pipeline;
            State = state;
            SurfaceShader = surfaceShader;
            BaseShader = baseShader;
        }
    }
}