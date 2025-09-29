namespace HexaEngine.ShadingLang
{
    using HexaEngine.Core.Graphics;

    public class HXSLPass : IHXSLName
    {
        public string Name { get; set; }

        public Dictionary<string, string> Tags { get; } = [];

        [HXSLName("Vertex")]
        public string? VertexShader { get; set; }

        [HXSLName("VertexEntryPoint")]
        public string VertexShaderEntryPoint { get; set; } = "main";

        [HXSLName("Hull")]
        public string? HullShader { get; set; }

        [HXSLName("HullEntryPoint")]
        public string HullShaderEntryPoint { get; set; } = "main";

        [HXSLName("Domain")]
        public string? DomainShader { get; set; }

        [HXSLName("DomainEntryPoint")]
        public string DomainShaderEntryPoint { get; set; } = "main";

        [HXSLName("Geometry")]
        public string? GeometryShader { get; set; }

        [HXSLName("GeometryEntryPoint")]
        public string GeometryShaderEntryPoint { get; set; } = "main";

        [HXSLName("Pixel")]
        public string? PixelShader { get; set; }

        [HXSLName("PixelEntryPoint")]
        public string PixelShaderEntryPoint { get; set; } = "main";

        [HXSLName("Compute")]
        public string? ComputeShader { get; set; }

        [HXSLName("ComputeEntryPoint")]
        public string ComputeShaderEntryPoint { get; set; } = "main";

        public HXSLPass()
        {
        }

        public BlendDescription Blend { get; set; }

        public DepthStencilDescription Depth { get; set; }

        public RasterizerDescription Rasterizer { get; set; }
    }
}