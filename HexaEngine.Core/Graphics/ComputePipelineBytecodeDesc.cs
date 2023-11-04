namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes the bytecode for a compute pipeline.
    /// </summary>
    public unsafe struct ComputePipelineBytecodeDesc
    {
        /// <summary>
        /// Gets or sets the pointer to the shader bytecode.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* Shader = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputePipelineBytecodeDesc"/> struct.
        /// </summary>
        public ComputePipelineBytecodeDesc()
        {
        }
    }
}