namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    public unsafe struct ComputePipelineBytecodeDesc
    {
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* Shader = null;

        public ComputePipelineBytecodeDesc()
        {
        }
    }
}