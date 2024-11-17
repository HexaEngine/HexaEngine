namespace HexaEngine.Graphics
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public struct LODGroupDesc : IXmlSerializable
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public LODFadeMode FadeMode { get; set; }

        public readonly XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement("LODGroup");
            string? guid = reader.GetAttribute("Guid");
            if (guid != null)
            {
                Guid = new Guid(guid);
            }
            string? name = reader.GetAttribute("Name");
            if (name != null)
            {
                Name = name;
            }
            string? fadeMode = reader.GetAttribute("FadeMode");
            if (fadeMode != null)
            {
                FadeMode = Enum.Parse<LODFadeMode>(fadeMode);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}