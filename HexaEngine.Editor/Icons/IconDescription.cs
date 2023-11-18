namespace HexaEngine.Editor.Icons
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Icon")]
    public class IconDescription
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "theme")]
        public string Theme { get; set; }

        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }

        [XmlAttribute(AttributeName = "target")]
        public string Target { get; set; }

        [XmlAttribute(AttributeName = "priority")]
        public int Priority { get; set; }
    }
}