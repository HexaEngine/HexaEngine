namespace HexaEngine.Editor.Projects
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public abstract class HexaProjectItem : IXmlSerializable
    {
        public abstract XmlSchema? GetSchema();

        public abstract void ReadXml(XmlReader reader);

        public abstract void WriteXml(XmlWriter writer);
    }

    public class HexaProject
    {
        private HexaProject()
        {
        }

        public static HexaProject CreateEmpty()
        {
            return new();
        }

        public static HexaProject CreateNew()
        {
            HexaProject project = new();
            PropertyGroup propertyGroup = [new("TargetVersion", "HexaEngine1.0")];
            project.Items.Add(propertyGroup);
            return project;
        }

        public List<HexaProjectItem> Items { get; } = [];
    }

    public class HexaProjectReader
    {
        private readonly XmlReader xmlReader;

        public HexaProjectReader(string inputUri)
        {
            xmlReader = XmlReader.Create(inputUri);
        }

        public HexaProjectReader(Stream stream)
        {
            xmlReader = XmlReader.Create(stream);
        }

        public HexaProjectReader(TextReader textReader)
        {
            xmlReader = XmlReader.Create(textReader);
        }

        public HexaProjectReader(XmlReader xmlReader)
        {
            this.xmlReader = xmlReader;
        }

        public HexaProject Read()
        {
            xmlReader.ReadStartElement("Project"); // <Project>
            HexaProject project = HexaProject.CreateEmpty();

            while (xmlReader.NodeType != XmlNodeType.EndElement)
            {
                xmlReader.Read();

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case "ItemGroup":
                            ItemGroup itemGroup = new();
                            itemGroup.ReadXml(xmlReader);
                            project.Items.Add(itemGroup);
                            break;

                        case "PropertyGroup":
                            PropertyGroup propertyGroup = new();
                            propertyGroup.ReadXml(xmlReader);
                            project.Items.Add(propertyGroup);
                            break;
                    }
                }
            }

            xmlReader.ReadEndElement(); // </Project>
            return project;
        }

        public void Close()
        {
            xmlReader.Close();
        }
    }

    public class HexaProjectWriter
    {
        private readonly XmlWriter xmlWriter;

        public HexaProjectWriter(string outputFile, bool indent = true)
        {
            xmlWriter = XmlWriter.Create(outputFile, new XmlWriterSettings() { Indent = indent, OmitXmlDeclaration = true });
        }

        public HexaProjectWriter(Stream stream, bool indent = true)
        {
            xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = indent, OmitXmlDeclaration = true });
        }

        public HexaProjectWriter(TextWriter textWriter, bool indent = true)
        {
            xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings() { Indent = indent, OmitXmlDeclaration = true });
        }

        public HexaProjectWriter(XmlWriter xmlWriter)
        {
            this.xmlWriter = xmlWriter;
        }

        public void Write(HexaProject project)
        {
            xmlWriter.WriteStartElement("Project");

            for (int i = 0; i < project.Items.Count; i++)
            {
                var item = project.Items[i];
                item.WriteXml(xmlWriter);
            }

            xmlWriter.WriteEndElement();
        }

        public void Flush()
        {
            xmlWriter.Flush();
        }

        public void Close()
        {
            xmlWriter.Close();
        }
    }
}