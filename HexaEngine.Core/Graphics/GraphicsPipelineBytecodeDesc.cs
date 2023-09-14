namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    public unsafe struct GraphicsPipelineBytecodeDesc
    {
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* VertexShader = null;

        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* HullShader = null;

        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* DomainShader = null;

        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* GeometryShader = null;

        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* PixelShader = null;

        public GraphicsPipelineBytecodeDesc()
        {
        }

        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* hullShader = null, Shader* domainShader = null, Shader* geometryShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            HullShader = hullShader;
            DomainShader = domainShader;
            GeometryShader = geometryShader;
            PixelShader = pixelShader;
        }

        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* hullShader = null, Shader* domainShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            HullShader = hullShader;
            DomainShader = domainShader;
            PixelShader = pixelShader;
        }

        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* geometryShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            GeometryShader = geometryShader;
            PixelShader = pixelShader;
        }

        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            PixelShader = pixelShader;
        }
    }
}