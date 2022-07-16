namespace HexaEngine.Graphics
{
    public struct PipelineDesc
    {
        public PipelineDesc()
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