namespace HexaEngine.Dotnet
{
    using System.Xml.Serialization;

    public class ItemGroup
    {
        [XmlElement(ElementName = "Reference")]
        public List<Reference> Reference { get; set; } = new();
    }
}