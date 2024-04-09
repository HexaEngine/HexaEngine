namespace HexaEngine.Editor.Packaging
{
    using HexaEngine.Editor.Projects;
    using System.Xml;
    using System.Xml.Schema;

    public class PackageReference : IItemGroupItem
    {
        public PackageReference()
        {
        }

        public PackageReference(string include, string version)
        {
            Include = include;
            Version = version;
        }

        public string Include { get; set; }

        public string Version { get; set; }

        public string Name { get; } = nameof(PackageReference);

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            Include = reader.GetAttribute("Include");
            Version = reader.GetAttribute("Version");
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Include", Include);
            writer.WriteAttributeString("Version", Version);
        }
    }
}