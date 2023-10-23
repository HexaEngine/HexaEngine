namespace HexaEngine.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class AZConfigKeyComparer : IComparer<ConfigKey>
    {
        public static readonly AZConfigKeyComparer Default = new();

        public int Compare(ConfigKey? x, ConfigKey? y)
        {
            return StringComparer.Ordinal.Compare(x.Name, y.Name);
        }
    }

    public class AZConfigValueComparer : IComparer<ConfigValue>
    {
        public static readonly AZConfigValueComparer Default = new();

        public int Compare(ConfigValue? x, ConfigValue? y)
        {
            return StringComparer.Ordinal.Compare(x.Name, y.Name);
        }
    }

    public class Config
    {
        private readonly object _lock = new();
        private static Config? global;

        public List<ConfigKey> Keys { get; set; } = new();

        [JsonIgnore]
        public object SyncObject => _lock;

        public void Sort()
        {
            lock (_lock)
            {
                Keys.Sort(AZConfigKeyComparer.Default);
            }
        }

        public ConfigKey? GetKey(string path)
        {
            lock (_lock)
            {
                return ConfigKey.Find(Keys, path);
            }
        }

        public ConfigKey GetOrCreateKey(string path)
        {
            lock (_lock)
            {
                ConfigKey? key = ConfigKey.Find(Keys, path);
                if (key is null)
                {
                    key = new ConfigKey(path);
                    Keys.Add(key);
                }
                return key;
            }
        }

        public ConfigKey GenerateSubKeyAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t, string name)
        {
            lock (_lock)
            {
                ConfigKey? key = ConfigKey.Find(Keys, name);
                bool isNew = key == null;
                key ??= new(name);
                key.InitAuto(t);
                if (isNew)
                {
                    Keys.Add(key);
                }
                return key;
            }
        }

        public static Config Global
        {
            get
            {
                if (global is null)
                {
                    if (File.Exists("config.json"))
                    {
                        global = (Config?)JsonSerializer.Deserialize(File.ReadAllText("config.json"), typeof(Config), SourceGenerationContext.Default) ?? throw new("Couldn't load config");
                    }
                    else
                    {
                        global = new Config();
                        global.Save();
                    }
                }

                return global;
            }
        }

        public void Save()
        {
            lock (_lock)
            {
                File.WriteAllText("config.json", JsonSerializer.Serialize(this, typeof(Config), SourceGenerationContext.Default));
            }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Config))]
    [JsonSerializable(typeof(ConfigKey))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}