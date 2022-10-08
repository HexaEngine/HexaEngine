namespace HexaEngine.Core
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class Config
    {
        public bool VSync { get; set; }

        public void Save()
        {
            File.WriteAllText("config.json", JsonSerializer.Serialize(this, typeof(Config), SourceGenerationContext.Default));
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Config))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}