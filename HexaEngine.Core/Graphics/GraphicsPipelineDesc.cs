namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a description of a graphics pipeline.
    /// </summary>
    public struct GraphicsPipelineDesc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineDesc"/> struct.
        /// </summary>
        public GraphicsPipelineDesc()
        {
        }

        /// <summary>
        /// Gets or sets the path to the vertex shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? VertexShader = null;

        /// <summary>
        /// Gets or sets the entry point for the vertex shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string VertexShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the hull shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? HullShader = null;

        /// <summary>
        /// Gets or sets the entry point for the hull shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string HullShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the domain shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? DomainShader = null;

        /// <summary>
        /// Gets or sets the entry point for the domain shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string DomainShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the geometry shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? GeometryShader = null;

        /// <summary>
        /// Gets or sets the entry point for the geometry shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string GeometryShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the pixel shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? PixelShader = null;

        /// <summary>
        /// Gets or sets the entry point for the pixel shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string PixelShaderEntrypoint = "main";
    }
}