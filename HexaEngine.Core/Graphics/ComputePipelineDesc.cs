namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    public struct ComputePipelineDesc
    {
        [XmlAttribute]
        [DefaultValue(null)]
        public string? Path;

        [XmlAttribute]
        [DefaultValue("main")]
        public string Entry = "main";

        public ComputePipelineDesc()
        {
            Path = null;
            Entry = "main";
        }

        public ComputePipelineDesc(string? path) : this()
        {
            Path = path;
        }

        public ComputePipelineDesc(string? path, string entry)
        {
            Path = path;
            Entry = entry;
        }
    }
}