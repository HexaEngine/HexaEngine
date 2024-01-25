namespace HexaEngine.Core
{
    using HexaEngine.Core.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Provides a custom comparer for ConfigKey objects based on their Name property.
    /// </summary>
    public class AZConfigKeyComparer : IComparer<ConfigKey>
    {
        /// <summary>
        /// Gets the default instance of the AZConfigKeyComparer.
        /// </summary>
        public static readonly AZConfigKeyComparer Instance = new();

        /// <summary>
        /// Compares two ConfigKey objects based on their Name property using StringComparer.Ordinal.Compare.
        /// </summary>
        /// <param name="x">The first ConfigKey to compare.</param>
        /// <param name="y">The second ConfigKey to compare.</param>
        /// <returns>
        /// A value less than 0 if x's Name is less than y's Name, 0 if they are equal, and a value greater than 0 if x's Name is greater than y's Name.
        /// </returns>
        public int Compare(ConfigKey? x, ConfigKey? y)
        {
            return StringComparer.Ordinal.Compare(x?.Name, y?.Name);
        }
    }

    /// <summary>
    /// Provides a custom comparer for ConfigValue objects based on their Name property.
    /// </summary>
    public class AZConfigValueComparer : IComparer<ConfigValue>
    {
        /// <summary>
        /// Gets the default instance of the AZConfigValueComparer.
        /// </summary>
        public static readonly AZConfigValueComparer Instance = new();

        /// <summary>
        /// Compares two ConfigValue objects based on their Name property using StringComparer.Ordinal.Compare.
        /// </summary>
        /// <param name="x">The first ConfigValue to compare.</param>
        /// <param name="y">The second ConfigValue to compare.</param>
        /// <returns>
        /// A value less than 0 if x's Name is less than y's Name, 0 if they are equal, and a value greater than 0 if x's Name is greater than y's Name.
        /// </returns>
        public int Compare(ConfigValue? x, ConfigValue? y)
        {
            return StringComparer.Ordinal.Compare(x?.Name, y?.Name);
        }
    }

    /// <summary>
    /// Provides a custom comparer for ConfigKey objects based on their Name property.
    /// </summary>
    public class ZAConfigKeyComparer : IComparer<ConfigKey>
    {
        /// <summary>
        /// Gets the default instance of the ZAConfigKeyComparer.
        /// </summary>
        public static readonly ZAConfigKeyComparer Instance = new();

        /// <summary>
        /// Compares two ConfigKey objects based on their Name property using StringComparer.Ordinal.Compare.
        /// </summary>
        /// <param name="x">The first ConfigKey to compare.</param>
        /// <param name="y">The second ConfigKey to compare.</param>
        /// <returns>
        /// A value less than 0 if x's Name is less than y's Name, 0 if they are equal, and a value greater than 0 if x's Name is greater than y's Name (sorting in reverse order).
        /// </returns>
        public int Compare(ConfigKey? x, ConfigKey? y)
        {
            return StringComparer.Ordinal.Compare(x?.Name, y?.Name) * -1;
        }
    }

    /// <summary>
    /// Provides a custom comparer for ConfigValue objects based on their Name property.
    /// </summary>
    public class ZAConfigValueComparer : IComparer<ConfigValue>
    {
        /// <summary>
        /// Gets the default instance of the ZAConfigValueComparer.
        /// </summary>
        public static readonly ZAConfigValueComparer Instance = new();

        /// <summary>
        /// Compares two ConfigValue objects based on their Name property using StringComparer.Ordinal.Compare.
        /// </summary>
        /// <param name="x">The first ConfigValue to compare.</param>
        /// <param name="y">The second ConfigValue to compare.</param>
        /// <returns>
        /// A value less than 0 if x's Name is less than y's Name, 0 if they are equal, and a value greater than 0 if x's Name is greater than y's Name (sorting in reverse order).
        /// </returns>
        public int Compare(ConfigValue? x, ConfigValue? y)
        {
            return StringComparer.Ordinal.Compare(x?.Name, y?.Name) * -1;
        }
    }

    /// <summary>
    /// Represents a configuration manager for handling a list of configuration keys.
    /// </summary>
    public class Config
    {
        private readonly object _lock = new();
        private static Config? global;

        /// <summary>
        /// Gets or sets the list of configuration keys contained within this configuration.
        /// </summary>
        public List<ConfigKey> Keys { get; set; } = new();

        /// <summary>
        /// Gets the synchronization object used for locking concurrent access to this configuration.
        /// </summary>
        [JsonIgnore]
        public object SyncObject => _lock;

        /// <summary>
        /// Sorts the list of configuration keys based on their names.
        /// </summary>
        public void Sort()
        {
            lock (_lock)
            {
                Keys.Sort(AZConfigKeyComparer.Instance);
            }
        }

        /// <summary>
        /// Sorts the list of configuration keys based on the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer to use for sorting the configuration keys.</param>
        public void Sort(IComparer<ConfigKey> comparer)
        {
            lock (_lock)
            {
                Keys.Sort(comparer);
            }
        }

        /// <summary>
        /// Gets a configuration key with the specified path.
        /// </summary>
        /// <param name="path">The path to the configuration key.</param>
        /// <returns>The configuration key with the specified path, or null if not found.</returns>
        public ConfigKey? GetKey(string path)
        {
            lock (_lock)
            {
                return ConfigKey.Find(Keys, path);
            }
        }

        /// <summary>
        /// Gets or creates a configuration key with the specified path.
        /// </summary>
        /// <param name="path">The path to the configuration key.</param>
        /// <returns>The configuration key with the specified path or a new one if it doesn't exist.</returns>
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

        /// <summary>
        /// Generates or retrieves a subkey automatically based on the properties of the specified object.
        /// </summary>
        /// <typeparam name="T">The type of object used to generate the subkey.</typeparam>
        /// <param name="t">The object used to generate the subkey.</param>
        /// <param name="name">The name of the subkey.</param>
        /// <returns>The generated or retrieved subkey.</returns>
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

        /// <summary>
        /// Gets the global configuration instance, loading it from "config.json" if it exists, or creating a new one if not.
        /// </summary>
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

        /// <summary>
        /// Saves the configuration to a "config.json" file.
        /// </summary>
        public void Save()
        {
            lock (_lock)
            {
                File.WriteAllText("config.json", JsonSerializer.Serialize(this, typeof(Config), SourceGenerationContext.Default));
            }
        }

        /// <summary>
        /// Saves the configuration to the specified path.
        /// </summary>
        /// <param name="path">The path to the config.</param>
        public void SaveTo(string path)
        {
            lock (_lock)
            {
                File.WriteAllText(path, JsonSerializer.Serialize(this, typeof(Config), SourceGenerationContext.Default));
            }
        }

        /// <summary>
        /// Loads a configuration from the specified path.
        /// </summary>
        /// <param name="path">The path to the config.</param>
        public static Config LoadFrom(string path)
        {
            Config config;
            if (File.Exists(path))
            {
                config = (Config?)JsonSerializer.Deserialize(File.ReadAllText(path), typeof(Config), SourceGenerationContext.Default) ?? new();
            }
            else
            {
                config = new Config();
                config.Save();
            }

            return config;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Config))]
    [JsonSerializable(typeof(ConfigKey))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}