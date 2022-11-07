namespace HexaEngine.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class Config
    {
        private static Config? global;

        public List<ConfigKey> Keys { get; set; } = new();

        public ConfigKey? GetKey(string path)
        {
            return Keys.Find(x => x.Name == path);
        }

        public ConfigKey GetOrCreateKey(string path)
        {
            ConfigKey? key = Keys.Find(x => x.Name == path);
            if (key is null)
            {
                key = new ConfigKey(path);
                Keys.Add(key);
            }
            return key;
        }

        public static Config Global
        {
            get
            {
                if (global is null)
                {
                    if (File.Exists("config.json"))
                    {
                        global = (Config?)JsonSerializer.Deserialize(File.ReadAllText("config.json"), typeof(Config), SourceGenerationContext.Default) ?? throw new();
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
            File.WriteAllText("config.json", JsonSerializer.Serialize(this, typeof(Config), SourceGenerationContext.Default));
        }
    }

    public class ConfigValue
    {
        private string? value;

        public ConfigValue(string name, DataType dataType, string? value)
        {
            Name = name;
            DataType = dataType;
            Value = value;
        }

        public DataType DataType { get; set; }

        public string Name { get; set; }

        public string? Value
        {
            get => value;
            set
            {
                this.value = value;
                ValueChanged?.Invoke(this, value);
            }
        }

        public event Action<ConfigValue, string?>? ValueChanged;

        public byte GetUInt8()
        {
            if (Value == null)
                return default;
            return byte.Parse(Value);
        }

        public sbyte GetInt8()
        {
            if (Value == null)
                return default;
            return sbyte.Parse(Value);
        }

        public ushort GetUInt16()
        {
            if (Value == null)
                return default;
            return ushort.Parse(Value);
        }

        public short GetInt16()
        {
            if (Value == null)
                return default;
            return short.Parse(Value);
        }

        public uint GetUInt32()
        {
            if (Value == null)
                return default;
            return uint.Parse(Value);
        }

        public int GetInt32()
        {
            if (Value == null)
                return default;
            return int.Parse(Value);
        }

        public ulong GetUInt64()
        {
            if (Value == null)
                return default;
            return ulong.Parse(Value);
        }

        public long GetInt64()
        {
            if (Value == null)
                return default;
            return long.Parse(Value);
        }

        public float GetFloat()
        {
            if (Value == null)
                return default;
            return float.Parse(Value);
        }

        public double GetDouble()
        {
            if (Value == null)
                return default;
            return double.Parse(Value);
        }

        public bool GetBool()
        {
            if (Value == null)
                return default;
            return bool.Parse(Value);
        }

        public Vector2 GetVector2()
        {
            if (Value == null)
                return default;
            string trimed = Value.Trim('<', '>');
            string[] components = trimed.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return new(float.Parse(components[0]), float.Parse(components[1]));
        }

        public Vector3 GetVector3()
        {
            if (Value == null)
                return default;
            string trimed = Value.Trim('<', '>');
            string[] components = trimed.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return new(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
        }

        public Vector4 GetVector4()
        {
            if (Value == null)
                return default;
            string trimed = Value.Trim('<', '>');
            string[] components = trimed.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return new(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
        }

        public T Get<T>(Func<string?, T> convert)
        {
            return convert(Value);
        }
    }

    public class ConfigKey
    {
        public ConfigKey(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public List<ConfigValue> Values { get; set; } = new();

        public List<ConfigKey> Keys { get; set; } = new();

        public ConfigKey GetOrCreateKey(string path)
        {
            ConfigKey? key = Keys.Find(x => x.Name == path);
            if (key is null)
            {
                key = new ConfigKey(path);
                Keys.Add(key);
            }
            return key;
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            value = Values.Find(x => x.Name == key)?.Value;
            return value != null;
        }

        public bool TryGetKeyValue(string key, [NotNullWhen(true)] out ConfigValue? value)
        {
            value = Values.Find(x => x.Name == key);
            return value != null;
        }

        public void TryGetOrAddKeyValue(string key, string defaultValue, DataType type, out ConfigValue value)
        {
            var val = Values.Find(x => x.Name == key);
            if (val == null)
            {
                val = new ConfigValue(key, type, defaultValue);
                Values.Add(val);
            }
            value = val;
        }

        public bool ContainsKey(string key)
        {
            return Values.Any(x => x.Name == key);
        }

        public void AddValue(string key, DataType type, string? value)
        {
            Values.Add(new(key, type, value));
        }

        public int TryGet(string key, int defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Int32, defaultValue.ToString());
                return defaultValue;
            }
            if (value == null)
                return defaultValue;
            return int.Parse(value);
        }

        public float TryGet(string key, float defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Float, defaultValue.ToString());
                return defaultValue;
            }
            if (value == null)
                return defaultValue;
            return float.Parse(value);
        }

        public bool TryGet(string key, bool defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Bool, defaultValue.ToString());
                return defaultValue;
            }
            if (value == null)
                return defaultValue;
            return bool.Parse(value);
        }

        public Vector3 TryGet(string key, Vector3 defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Float3, defaultValue.ToString());
                return defaultValue;
            }

            if (value == null)
                return defaultValue;
            return default;
        }

        public T TryGet<T>(string key, T defaultValue, DataType type, Func<string?, T> convert, Func<T, string?> convertBack)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, type, convertBack(defaultValue));
                return defaultValue;
            }

            return convert(value);
        }

        public void SetValue(string key, string? value)
        {
            if (TryGetKeyValue(key, out var kvalue))
            {
                kvalue.Value = value;
            }
        }

        public void SetValue<T>(string key, T? value)
        {
            if (TryGetKeyValue(key, out var kvalue))
            {
                kvalue.Value = value?.ToString();
            }
        }

        public void GenerateSubKeyAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t, string name)
        {
            if (Keys.Any(x => x.Name == name)) return;
            ConfigKey key = new(name);
            key.GenerateAuto(t);
            Keys.Add(key);
        }

        public void GenerateAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                ConfigValue? val = null;
                if (property.PropertyType == typeof(string))
                {
                    val = new(property.Name, DataType.String, property.GetValue(t) as string);
                    val.ValueChanged += (s, e) =>
                    {
                        property.SetValue(t, e);
                    };
                }

                if (property.PropertyType == typeof(bool))
                {
                    val = new(property.Name, DataType.Bool, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, bool.Parse(e));
                        else
                            property.SetValue(t, default(bool));
                    };
                }

                if (property.PropertyType == typeof(byte))
                {
                    val = new(property.Name, DataType.UInt8, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, byte.Parse(e));
                        else
                            property.SetValue(t, default(byte));
                    };
                }

                if (property.PropertyType == typeof(sbyte))
                {
                    val = new(property.Name, DataType.Int8, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, sbyte.Parse(e));
                        else
                            property.SetValue(t, default(sbyte));
                    };
                }

                if (property.PropertyType == typeof(ushort))
                {
                    val = new(property.Name, DataType.UInt16, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, ushort.Parse(e));
                        else
                            property.SetValue(t, default(ushort));
                    };
                }

                if (property.PropertyType == typeof(short))
                {
                    val = new(property.Name, DataType.Int16, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, short.Parse(e));
                        else
                            property.SetValue(t, default(short));
                    };
                }

                if (property.PropertyType == typeof(uint))
                {
                    val = new(property.Name, DataType.UInt32, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, uint.Parse(e));
                        else
                            property.SetValue(t, default(uint));
                    };
                }

                if (property.PropertyType == typeof(int))
                {
                    val = new(property.Name, DataType.Int32, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, int.Parse(e));
                        else
                            property.SetValue(t, default(int));
                    };
                }

                if (property.PropertyType == typeof(ulong))
                {
                    val = new(property.Name, DataType.UInt64, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, ulong.Parse(e));
                        else
                            property.SetValue(t, default(ulong));
                    };
                }

                if (property.PropertyType == typeof(long))
                {
                    val = new(property.Name, DataType.Int64, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, long.Parse(e));
                        else
                            property.SetValue(t, default(long));
                    };
                }

                if (property.PropertyType == typeof(float))
                {
                    val = new(property.Name, DataType.Float, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, float.Parse(e));
                        else
                            property.SetValue(t, default(float));
                    };
                }

                if (property.PropertyType == typeof(double))
                {
                    val = new(property.Name, DataType.Double, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, double.Parse(e));
                        else
                            property.SetValue(t, default(double));
                    };
                }

                if (property.PropertyType == typeof(Vector2))
                {
                    val = new(property.Name, DataType.Float2, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, s.GetVector2());
                        else
                            property.SetValue(t, default(Vector2));
                    };
                }

                if (property.PropertyType == typeof(Vector3))
                {
                    val = new(property.Name, DataType.Float3, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, s.GetVector3());
                        else
                            property.SetValue(t, default(Vector3));
                    };
                }

                if (property.PropertyType == typeof(Vector4))
                {
                    val = new(property.Name, DataType.Float4, property.GetValue(t)?.ToString());
                    val.ValueChanged += (s, e) =>
                    {
                        if (e != null)
                            property.SetValue(t, s.GetVector4());
                        else
                            property.SetValue(t, default(Vector4));
                    };
                }

                if (val == null) continue;
                Values.Add(val);
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