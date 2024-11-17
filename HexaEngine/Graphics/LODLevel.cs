namespace HexaEngine.Graphics
{
    using System.Globalization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public struct LODLevel : IXmlSerializable
    {
        public int LODIndex;
        public float MaxDistance;

        public readonly XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement("LODLevel");

            string? lodIndex = reader.GetAttribute("LODIndex");
            if (lodIndex != null)
            {
                LODIndex = int.Parse(lodIndex);
            }
            string? maxDistance = reader.GetAttribute("MaxDistance");
            if (maxDistance != null)
            {
                MaxDistance = float.Parse(maxDistance, NumberStyles.Any, CultureInfo.InvariantCulture);
            }

            reader.ReadEndElement();
        }

        public readonly void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("LODLevel");
            writer.WriteAttributeString("LODIndex", LODIndex.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("MaxDistance", MaxDistance.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndAttribute();
        }
    }
}