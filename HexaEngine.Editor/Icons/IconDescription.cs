namespace HexaEngine.Editor.Icons
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Icon")]
    public class IconDescription
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; } = null!;

        [XmlAttribute(AttributeName = "theme")]
        public string Theme { get; set; } = null!;

        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; } = null!;

        [XmlAttribute(AttributeName = "target")]
        public string Target { get; set; } = null!;

        [XmlAttribute(AttributeName = "priority")]
        public int Priority { get; set; }

        [DefaultValue(null)]
        [XmlAttribute(AttributeName = "tint")]
        public string? Tint { get; set; }
    }
}