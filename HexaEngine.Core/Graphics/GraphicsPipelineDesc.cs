namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    public struct GraphicsPipelineDesc
    {
        public GraphicsPipelineDesc()
        {
        }

        [XmlAttribute]
        [DefaultValue(null)]
        public string? VertexShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string VertexShaderEntrypoint = "main";

        [XmlAttribute]
        [DefaultValue(null)]
        public string? HullShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string HullShaderEntrypoint = "main";

        [XmlAttribute]
        [DefaultValue(null)]
        public string? DomainShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string DomainShaderEntrypoint = "main";

        [XmlAttribute]
        [DefaultValue(null)]
        public string? GeometryShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string GeometryShaderEntrypoint = "main";

        [XmlAttribute]
        [DefaultValue(null)]
        public string? PixelShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string PixelShaderEntrypoint = "main";
    }
}