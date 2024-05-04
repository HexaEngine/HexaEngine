namespace HexaEngine
{
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class AppConfig : IXmlSerializable
    {
        private static readonly XmlWriterSettings writerSettings = new() { OmitXmlDeclaration = true, Indent = true };
        private readonly string path;

        public AppConfig(string path)
        {
            this.path = path;
        }

        public string? AppName { get; set; }

        public string StartupScene { get; set; } = "";

        public string ScriptAssembly { get; set; } = "";

        public Dictionary<string, string> Variables { get; set; } = [];

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element)
            {
                AppName = reader.GetAttribute("AppName");
                StartupScene = reader.GetAttribute("StartupScene");
                ScriptAssembly = reader.GetAttribute("ScriptAssembly");
            }

            reader.ReadStartElement();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "AppConfig")
                    break;

                if (reader.MoveToContent() == XmlNodeType.Element)
                {
                    string key = reader.Name;
                    string value = reader.ReadElementContentAsString();
                    Variables.Add(key, value);
                    reader.ReadEndElement();
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("AppConfig");
            if (AppName != null)
            {
                writer.WriteAttributeString("AppName", AppName);
            }

            if (!string.IsNullOrEmpty(StartupScene))
            {
                writer.WriteAttributeString("StartupScene", StartupScene);
            }

            if (!string.IsNullOrEmpty(ScriptAssembly))
            {
                writer.WriteAttributeString("ScriptAssembly", ScriptAssembly);
            }

            foreach (var kvp in Variables)
            {
                writer.WriteStartElement(kvp.Key);
                writer.WriteString(kvp.Value);
                writer.WriteEndElement(); // End of Variable element
            }

            writer.WriteEndElement(); // End of AppConfig element
        }

        public static AppConfig Load(string path)
        {
            var result = new AppConfig(path);

            if (File.Exists(path))
            {
                FileStream? stream = null;
                XmlReader? reader = null;
                try
                {
                    stream = File.OpenRead(path);
                    reader = XmlReader.Create(stream);
                    result.ReadXml(reader);
                }
                finally
                {
                    reader?.Close();
                    stream?.Close();
                }
            }
            else
            {
                result.Save();
            }

            return result;
        }

        public void Save()
        {
            FileStream? stream = null;
            XmlWriter? writer = null;
            try
            {
                stream = File.Create(path);
                writer = XmlWriter.Create(stream, writerSettings);
                WriteXml(writer);
            }
            finally
            {
                writer?.Close();
                stream?.Close();
            }
        }

        public void SaveTo(string path)
        {
            FileStream? stream = null;
            XmlWriter? writer = null;
            try
            {
                stream = File.Create(path);
                writer = XmlWriter.Create(stream, writerSettings);
                WriteXml(writer);
            }
            finally
            {
                writer?.Close();
                stream?.Close();
            }
        }

        public AppConfig Clone(string path)
        {
            return new AppConfig(path)
            {
                AppName = AppName,
                ScriptAssembly = ScriptAssembly,
                StartupScene = StartupScene,
                Variables = Variables.ToDictionary()
            };
        }
    }
}