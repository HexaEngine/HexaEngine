namespace HexaEngine.Projects
{
    using System.Xml.Serialization;

    public class AppConfig
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));

        public string StartupScene { get; set; } = "";

        public string ScriptAssembly { get; set; } = "";

        public static AppConfig Load()
        {
            AppConfig result;
            if (File.Exists("app.config"))
            {
                var stream = File.OpenRead("app.config");
                result = (AppConfig?)serializer.Deserialize(stream) ?? throw new InvalidDataException();
                stream.Close();
            }
            else
            {
                result = new();
                var stream = File.Create("app.config");
                serializer.Serialize(stream, result);
                stream.Close();
            }
            return result;
        }

        public void Save(string path)
        {
            var stream = File.Create(path);
            serializer.Serialize(stream, this);
            stream.Close();
        }
    }
}