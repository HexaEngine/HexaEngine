namespace HexaEngine.Dotnet
{
    using System.Xml.Serialization;

    public class Reference
    {
        [XmlElement(ElementName = "HintPath")]
        public string HintPath { get; set; }

        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }
    }
}