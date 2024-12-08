namespace HexaEngine.Input
{
    using HexaEngine.Core.Assets;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class InputMap : IXmlSerializable
    {
        private static readonly XmlWriterSettings writerSettings = new() { OmitXmlDeclaration = true, Indent = true };

        public InputMap()
        {
        }

        public InputMap(string name)
        {
            Name = name;
        }

        [XmlAttribute]
        public string Name { get; set; } = null!;

        public List<VirtualAxis> VirtualAxes { get; } = new();

        public XmlSchema? GetSchema()
        {
            return null; // Not needed
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            Name = reader.GetAttribute("Name")!;

            reader.ReadStartElement();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "InputMap")
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "VirtualAxes")
                {
                    if (reader.IsEmptyElement)
                    {
                        continue;
                    }

                    reader.ReadStartElement("VirtualAxes");

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "VirtualAxes")
                        {
                            break;
                        }

                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "VirtualAxis")
                        {
                            VirtualAxis axis = new();
                            axis.ReadXml(reader);
                            VirtualAxes.Add(axis);
                        }
                    }

                    reader.ReadEndElement(); // End of VirtualAxes element
                }
            }

            reader.ReadEndElement(); // End of InputMap element
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("InputMap");
            writer.WriteAttributeString("Name", Name);

            writer.WriteStartElement("VirtualAxes");
            foreach (var axis in VirtualAxes)
            {
                writer.WriteStartElement("VirtualAxis");
                axis.WriteXml(writer);
                writer.WriteEndElement(); // End of VirtualAxis element
            }
            writer.WriteEndElement(); // End of VirtualAxes element

            writer.WriteEndElement(); // End of InputMap element
        }

        public void Save(string path)
        {
            XmlWriter writer = XmlWriter.Create(path, writerSettings);
            try
            {
                WriteXml(writer);
            }
            finally
            {
                writer.Close();
            }
        }

        public string SaveAsText()
        {
            StringWriter writer = new();
            XmlWriter xmlWriter = XmlWriter.Create(writer, writerSettings);
            try
            {
                WriteXml(xmlWriter);
            }
            finally
            {
                xmlWriter.Close();
            }

            return writer.ToString();
        }

        public static InputMap Load(string path)
        {
            InputMap inputMap;
            XmlReader xmlReader = XmlReader.Create(path);
            try
            {
                inputMap = new();
                inputMap.ReadXml(xmlReader);
            }
            finally
            {
                xmlReader.Close();
            }

            return inputMap;
        }

        public static InputMap LoadFromText(string text)
        {
            InputMap inputMap;
            TextReader reader = new StringReader(text);
            XmlReader xmlReader = XmlReader.Create(reader);
            try
            {
                inputMap = new();
                inputMap.ReadXml(xmlReader);
            }
            finally
            {
                xmlReader.Close();
                reader.Close();
            }

            return inputMap;
        }

        public static InputMap? Load(AssetRef asset)
        {
            InputMap inputMap;
            Stream? fs = asset.OpenRead();

            if (fs == null)
            {
                return null;
            }

            XmlReader xmlReader = XmlReader.Create(fs);
            try
            {
                inputMap = new();
                inputMap.ReadXml(xmlReader);
            }
            finally
            {
                xmlReader.Close();
            }

            return inputMap;
        }
    }
}