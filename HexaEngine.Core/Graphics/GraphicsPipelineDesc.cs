namespace HexaEngine.Core.Graphics
{
    public struct GraphicsPipelineDesc
    {
        public GraphicsPipelineDesc()
        {
        }

        public string? VertexShader = null;
        public string VertexShaderEntrypoint = "main";
        public string? HullShader = null;
        public string HullShaderEntrypoint = "main";
        public string? DomainShader = null;
        public string DomainShaderEntrypoint = "main";
        public string? GeometryShader = null;
        public string GeometryShaderEntrypoint = "main";
        public string? PixelShader = null;
        public string PixelShaderEntrypoint = "main";
    }
}