namespace HexaEngine.Editor.Icons
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Icons")]
    public class IconsDescription
    {
        [XmlElement(ElementName = "Icon")]
        public List<IconDescription> Icons { get; set; } = null!;
    }
}