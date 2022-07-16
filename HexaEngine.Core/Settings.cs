namespace HexaEngine.Core
{
    using Newtonsoft.Json;

    public class Settings
    {
        [JsonProperty]
        public bool VSync;

        [JsonProperty]
        public bool LimitFPS = true;

        [JsonProperty]
        public int TargetFPS = 120;

        [JsonProperty]
        public int ShadowMapSize = 1024 * 8;

        [JsonProperty]
        public int ShadowRange = 1024 / 8;

        [JsonProperty]
        public int MipLevels = 8;

        [JsonProperty]
        public bool ShaderCache = true;

        [JsonIgnore]
        public int BufferCount = 2;

        [JsonIgnore]
        public bool MSAA;

        [JsonIgnore]
        public int MSAASampleCount = 1;

        [JsonIgnore]
        public int MSAASampleQuality;

        [JsonIgnore]
        public float MaxDepth = 1000f;

        [JsonIgnore]
        public float MinDepth = .001f;

        internal void Save()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public static class Nucleus
    {
        static Nucleus()
        {
            if (File.Exists("config.json"))
            {
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.json")) ?? new();
            }
            else
            {
                Settings = new();
                Settings.Save();
            }
        }

        public static Settings Settings { get; }
    }
}