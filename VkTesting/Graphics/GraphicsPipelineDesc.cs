namespace HexaEngine.Vulkan
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
        public string? TessControlShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string TessControlShaderEntrypoint = "main";

        [XmlAttribute]
        [DefaultValue(null)]
        public string? TessEvaluationShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string TessEvaluationShaderEntrypoint = "main";

        [XmlAttribute]
        [DefaultValue(null)]
        public string? GeometryShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string GeometryShaderEntrypoint = "main";

        [XmlAttribute]
        [DefaultValue(null)]
        public string? FragmentShader = null;

        [XmlAttribute]
        [DefaultValue("main")]
        public string FragmentShaderEntrypoint = "main";
    }
}