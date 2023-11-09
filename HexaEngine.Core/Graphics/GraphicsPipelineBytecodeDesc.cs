namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a description of the bytecode for a graphics pipeline.
    /// </summary>
    public unsafe struct GraphicsPipelineBytecodeDesc
    {
        /// <summary>
        /// Gets or sets the vertex shader bytecode.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* VertexShader = null;

        /// <summary>
        /// Gets or sets the hull shader bytecode.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* HullShader = null;

        /// <summary>
        /// Gets or sets the domain shader bytecode.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* DomainShader = null;

        /// <summary>
        /// Gets or sets the geometry shader bytecode.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* GeometryShader = null;

        /// <summary>
        /// Gets or sets the pixel shader bytecode.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* PixelShader = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineBytecodeDesc"/> struct.
        /// </summary>
        public GraphicsPipelineBytecodeDesc()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineBytecodeDesc"/> struct with specified shaders.
        /// </summary>
        /// <param name="vertexShader">The vertex shader bytecode.</param>
        /// <param name="hullShader">The hull shader bytecode.</param>
        /// <param name="domainShader">The domain shader bytecode.</param>
        /// <param name="geometryShader">The geometry shader bytecode.</param>
        /// <param name="pixelShader">The pixel shader bytecode.</param>
        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* hullShader = null, Shader* domainShader = null, Shader* geometryShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            HullShader = hullShader;
            DomainShader = domainShader;
            GeometryShader = geometryShader;
            PixelShader = pixelShader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineBytecodeDesc"/> struct with specified shaders.
        /// </summary>
        /// <param name="vertexShader">The vertex shader bytecode.</param>
        /// <param name="hullShader">The hull shader bytecode.</param>
        /// <param name="domainShader">The domain shader bytecode.</param>
        /// <param name="pixelShader">The pixel shader bytecode.</param>
        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* hullShader = null, Shader* domainShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            HullShader = hullShader;
            DomainShader = domainShader;
            PixelShader = pixelShader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineBytecodeDesc"/> struct with specified shaders.
        /// </summary>
        /// <param name="vertexShader">The vertex shader bytecode.</param>
        /// <param name="geometryShader">The geometry shader bytecode.</param>
        /// <param name="pixelShader">The pixel shader bytecode.</param>
        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* geometryShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            GeometryShader = geometryShader;
            PixelShader = pixelShader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineBytecodeDesc"/> struct with specified shaders.
        /// </summary>
        /// <param name="vertexShader">The vertex shader bytecode.</param>
        /// <param name="pixelShader">The pixel shader bytecode.</param>
        public GraphicsPipelineBytecodeDesc(Shader* vertexShader = null, Shader* pixelShader = null)
        {
            VertexShader = vertexShader;
            PixelShader = pixelShader;
        }
    }
}