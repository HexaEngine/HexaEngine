namespace HexaEngine.Editor
{
    using System.Xml.Serialization;

    [XmlRoot("Color")]
    public class ColorEntry
    {
        [XmlAttribute("Name")]
        public ThemeColor Name { get; set; }

        [XmlElement("Value")]
        public ColorRGBA Value { get; set; }
    }
}