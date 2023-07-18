namespace HexaEngine.Effects
{
    using HexaEngine.Core.IO;
    using System.Xml.Serialization;

    public class EffectDescriptionFile
    {
        private static readonly XmlSerializer xmlSerializer = new(typeof(EffectDescriptionFile));
        public const int CurrentVersion = 1;

        public int Version { get; set; } = CurrentVersion;

        public EffectDescription Description { get; set; } = new();

        public static void Create(string path)
        {
            var fs = File.Create(path);
            EffectDescriptionFile file = new();
            xmlSerializer.Serialize(fs, file);
            fs.Close();
        }

        public static EffectDescriptionFile Load(string path)
        {
            var fs = FileSystem.Open(path);
            var result = (EffectDescriptionFile)(xmlSerializer.Deserialize(fs) ?? throw new());
            fs.Close();
            return result;
        }

        public static EffectDescriptionFile LoadExternal(string path)
        {
            var fs = File.OpenRead(path);
            var result = (EffectDescriptionFile)(xmlSerializer.Deserialize(fs) ?? throw new());
            fs.Close();
            return result;
        }
    }
}