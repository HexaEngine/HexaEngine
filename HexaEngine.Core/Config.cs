namespace HexaEngine.Core
{
    using HexaEngine.Core.Input;
    using Newtonsoft.Json.Linq;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
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
            return ConfigKey.Find(Keys, path);
        }

        public ConfigKey GetOrCreateKey(string path)
        {
            ConfigKey? key = ConfigKey.Find(Keys, path);
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
        private object? instance;
        private PropertyInfo? property;

        private string? value;
        private string? defaultValue;

        [JsonConstructor]
        public ConfigValue(string name, DataType dataType, string? value, bool isReadOnly)
        {
            Name = name;
            DataType = dataType;
            Value = value;
            IsReadOnly = isReadOnly;
        }

        public ConfigValue(object instanceObject, PropertyInfo propertyInfo, DataType dataType, string? value, bool isReadOnly)
        {
            instance = instanceObject;
            property = propertyInfo;
            Name = propertyInfo.Name;
            DataType = dataType;
            Value = value;
            IsReadOnly = isReadOnly;
        }

        [JsonIgnore]
        public object? Instance { get => instance; internal set => instance = value; }

        [JsonIgnore]
        public PropertyInfo? Property { get => property; internal set => property = value; }

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

        public bool IsReadOnly { get; set; }

        [JsonIgnore]
        public string? DefaultValue { get => defaultValue; set => defaultValue = value; }

        public event Action<ConfigValue, string?>? ValueChanged;

        public void SetToDefault()
        {
            Value = defaultValue;
        }

        public byte GetUInt8()
        {
            if (Value == null)
            {
                return default;
            }

            return byte.Parse(Value);
        }

        public sbyte GetInt8()
        {
            if (Value == null)
            {
                return default;
            }

            return sbyte.Parse(Value);
        }

        public ushort GetUInt16()
        {
            if (Value == null)
            {
                return default;
            }

            return ushort.Parse(Value);
        }

        public short GetInt16()
        {
            if (Value == null)
            {
                return default;
            }

            return short.Parse(Value);
        }

        public uint GetUInt32()
        {
            if (Value == null)
            {
                return default;
            }

            return uint.Parse(Value);
        }

        public int GetInt32()
        {
            if (Value == null)
            {
                return default;
            }

            return int.Parse(Value);
        }

        public ulong GetUInt64()
        {
            if (Value == null)
            {
                return default;
            }

            return ulong.Parse(Value);
        }

        public long GetInt64()
        {
            if (Value == null)
            {
                return default;
            }

            return long.Parse(Value);
        }

        public float GetFloat()
        {
            if (Value == null)
            {
                return default;
            }

            return float.Parse(Value);
        }

        public double GetDouble()
        {
            if (Value == null)
            {
                return default;
            }

            return double.Parse(Value);
        }

        public bool GetBool()
        {
            if (Value == null)
            {
                return default;
            }

            return bool.Parse(Value);
        }

        public Vector2 GetVector2()
        {
            if (Value == null)
            {
                return default;
            }

            string trimed = Value.Trim('<', '>');
            string[] components = trimed.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < components.Length; i++)
            {
                components[i] = components[i].Trim(',', '.');
            }

            return new(float.Parse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture));
        }

        public Vector3 GetVector3()
        {
            if (Value == null)
            {
                return default;
            }

            string trimed = Value.Trim('<', '>');
            string[] components = trimed.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return new(float.Parse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture));
        }

        public Vector4 GetVector4()
        {
            if (Value == null)
            {
                return default;
            }

            string trimed = Value.Trim('<', '>');
            string[] components = trimed.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return new(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
        }

        public IEnumerable<KeyCode> GetKeys()
        {
            if (Value == null)
            {
                yield break;
            }

            string[] keys = Value.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keys.Length; i++)
            {
                yield return (KeyCode)Enum.Parse(typeof(KeyCode), keys[i]);
            }
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
            ConfigKey? key = Find(Keys, path);
            if (key is null)
            {
                key = new ConfigKey(path);
                Keys.Add(key);
            }
            return key;
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            value = Find(Values, key)?.Value;
            return value != null;
        }

        public bool TryGetKeyValue(string key, [NotNullWhen(true)] out ConfigValue? value)
        {
            value = Find(Values, key);
            return value != null;
        }

        public void TryGetOrAddKeyValue(string key, string? defaultValue, DataType type, bool isReadOnly, out ConfigValue value)
        {
            var val = Find(Values, key);
            if (val == null)
            {
                val = new ConfigValue(key, type, defaultValue, isReadOnly);
                Values.Add(val);
            }
            val.DefaultValue = defaultValue;
            value = val;
        }

        public bool ContainsKey(string key)
        {
            for (int i = 0; i < Values.Count; i++)
            {
                var val = Values[i];
                if (val.Name == key)
                    return true;
            }
            return false;
        }

        public static ConfigValue? Find(List<ConfigValue> values, string key)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var val = values[i];
                if (val.Name == key)
                    return val;
            }
            return null;
        }

        public void AddValue(string key, DataType type, string? value, bool isReadOnly)
        {
            ConfigValue configValue = new(key, type, value, isReadOnly);
            configValue.DefaultValue = value;
            Values.Add(configValue);
        }

        public int TryGet(string key, int defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Int32, defaultValue.ToString(), false);
                return defaultValue;
            }
            if (value == null)
            {
                return defaultValue;
            }

            return int.Parse(value);
        }

        public float TryGet(string key, float defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Float, defaultValue.ToString(), false);
                return defaultValue;
            }
            if (value == null)
            {
                return defaultValue;
            }

            return float.Parse(value);
        }

        public bool TryGet(string key, bool defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Bool, defaultValue.ToString(), false);
                return defaultValue;
            }
            if (value == null)
            {
                return defaultValue;
            }

            return bool.Parse(value);
        }

        public Vector3 TryGet(string key, Vector3 defaultValue)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, DataType.Float3, defaultValue.ToString(), false);
                return defaultValue;
            }

            if (value == null)
            {
                return defaultValue;
            }

            return default;
        }

        public T TryGet<T>(string key, T defaultValue, DataType type, Func<string?, T> convert, Func<T, string?> convertBack)
        {
            if (!TryGetValue(key, out string? value))
            {
                AddValue(key, type, convertBack(defaultValue), false);
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
            ConfigKey? key = Find(Keys, name);
            bool isNew = key == null;
            key ??= new(name);
            key.InitAuto(t);
            if (isNew)
            {
                Keys.Add(key);
            }
        }

        public static ConfigKey? Find(List<ConfigKey> keys, string name)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Name == name)
                    return keys[i];
            }
            return null;
        }

        private static void StringValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
            property.SetValue(t, e);
#nullable enable
        }

        private static void BoolValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, bool.Parse(e));
            }
            else
            {
                property.SetValue(t, default(bool));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void ByteValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, byte.Parse(e));
            }
            else
            {
                property.SetValue(t, default(byte));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void SByteValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, sbyte.Parse(e));
            }
            else
            {
                property.SetValue(t, default(sbyte));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void UShortValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, ushort.Parse(e));
            }
            else
            {
                property.SetValue(t, default(ushort));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void ShortValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, short.Parse(e));
            }
            else
            {
                property.SetValue(t, default(short));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void UIntValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, uint.Parse(e));
            }
            else
            {
                property.SetValue(t, default(uint));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void IntValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, int.Parse(e));
            }
            else
            {
                property.SetValue(t, default(int));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void ULongValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, ulong.Parse(e));
            }
            else
            {
                property.SetValue(t, default(ulong));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void LongValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, long.Parse(e));
            }
            else
            {
                property.SetValue(t, default(long));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void FloatValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, float.Parse(e));
            }
            else
            {
                property.SetValue(t, default(float));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void DoubleValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, double.Parse(e));
            }
            else
            {
                property.SetValue(t, default(double));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void Vector2ValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, configValue.GetVector2());
            }
            else
            {
                property.SetValue(t, default(Vector2));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void Vector3ValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, configValue.GetVector3());
            }
            else
            {
                property.SetValue(t, default(Vector3));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        private static void Vector4ValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var t = configValue.Instance;
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            if (e != null)
            {
                property.SetValue(t, configValue.GetVector4());
            }
            else
            {
                property.SetValue(t, default(Vector4));
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        public void GenerateAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t)
        {
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                ConfigValue? val = null;
                if (property.PropertyType == typeof(string))
                {
                    val = new(t, property, DataType.String, property.GetValue(t) as string, !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += StringValueChanged;
                }

                if (property.PropertyType == typeof(bool))
                {
                    val = new(t, property, DataType.Bool, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += BoolValueChanged;
                }

                if (property.PropertyType == typeof(byte))
                {
                    val = new(t, property, DataType.UInt8, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += ByteValueChanged;
                }

                if (property.PropertyType == typeof(sbyte))
                {
                    val = new(t, property, DataType.Int8, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += SByteValueChanged;
                }

                if (property.PropertyType == typeof(ushort))
                {
                    val = new(t, property, DataType.UInt16, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += UShortValueChanged;
                }

                if (property.PropertyType == typeof(short))
                {
                    val = new(property.Name, DataType.Int16, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += ShortValueChanged;
                }

                if (property.PropertyType == typeof(uint))
                {
                    val = new(t, property, DataType.UInt32, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += UIntValueChanged;
                }

                if (property.PropertyType == typeof(int))
                {
                    val = new(t, property, DataType.Int32, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += IntValueChanged;
                }

                if (property.PropertyType == typeof(ulong))
                {
                    val = new(t, property, DataType.UInt64, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += ULongValueChanged;
                }

                if (property.PropertyType == typeof(long))
                {
                    val = new(t, property, DataType.Int64, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += LongValueChanged;
                }

                if (property.PropertyType == typeof(float))
                {
                    val = new(t, property, DataType.Float, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += FloatValueChanged;
                }

                if (property.PropertyType == typeof(double))
                {
                    val = new(t, property, DataType.Double, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += DoubleValueChanged;
                }

                if (property.PropertyType == typeof(Vector2))
                {
                    val = new(t, property, DataType.Float2, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += Vector2ValueChanged;
                }

                if (property.PropertyType == typeof(Vector3))
                {
                    val = new(t, property, DataType.Float3, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += Vector3ValueChanged;
                }

                if (property.PropertyType == typeof(Vector4))
                {
                    val = new(t, property, DataType.Float4, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                        val.ValueChanged += Vector4ValueChanged;
                }

                if (val == null)
                {
                    continue;
                }

                Values.Add(val);
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }

        public void InitAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t)
        {
#nullable disable
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                TryGetKeyValue(property.Name, out ConfigValue val);

                bool isNew = val == null;
                if (property.PropertyType == typeof(string))
                {
                    val ??= new(t, property, DataType.String, property.GetValue(t) as string, !property.CanWrite);

                    if (property.CanWrite)
                    {
                        val.ValueChanged += StringValueChanged;
                    }
                }

                if (property.PropertyType == typeof(bool))
                {
                    val ??= new(t, property, DataType.Bool, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += BoolValueChanged;
                    }
                }

                if (property.PropertyType == typeof(byte))
                {
                    val ??= new(t, property, DataType.UInt8, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += ByteValueChanged;
                    }
                }

                if (property.PropertyType == typeof(sbyte))
                {
                    val ??= new(t, property, DataType.Int8, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += SByteValueChanged;
                    }
                }

                if (property.PropertyType == typeof(ushort))
                {
                    val ??= new(t, property, DataType.UInt16, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += UShortValueChanged;
                    }
                }

                if (property.PropertyType == typeof(short))
                {
                    val ??= new(t, property, DataType.Int16, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += ShortValueChanged;
                    }
                }

                if (property.PropertyType == typeof(uint))
                {
                    val ??= new(t, property, DataType.UInt32, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += UIntValueChanged;
                    }
                }

                if (property.PropertyType == typeof(int))
                {
                    val ??= new(t, property, DataType.Int32, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += IntValueChanged;
                    }
                }

                if (property.PropertyType == typeof(ulong))
                {
                    val ??= new(t, property, DataType.UInt64, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += ULongValueChanged;
                    }
                }

                if (property.PropertyType == typeof(long))
                {
                    val ??= new(t, property, DataType.Int64, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += LongValueChanged;
                    }
                }

                if (property.PropertyType == typeof(float))
                {
                    val ??= new(t, property, DataType.Float, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += FloatValueChanged;
                    }
                }

                if (property.PropertyType == typeof(double))
                {
                    val ??= new(t, property, DataType.Double, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += DoubleValueChanged;
                    }
                }

                if (property.PropertyType == typeof(Vector2))
                {
                    val ??= new(t, property, DataType.Float2, ((Vector2)property.GetValue(t)).ToString("G", CultureInfo.InvariantCulture), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += Vector2ValueChanged;
                    }
                }

                if (property.PropertyType == typeof(Vector3))
                {
                    val ??= new(t, property, DataType.Float3, ((Vector3)property.GetValue(t)).ToString("G", CultureInfo.InvariantCulture), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += Vector3ValueChanged;
                    }
                }

                if (property.PropertyType == typeof(Vector4))
                {
                    val ??= new(t, property, DataType.Float4, property.GetValue(t)?.ToString(), !property.CanWrite);
                    if (property.CanWrite)
                    {
                        val.ValueChanged += Vector4ValueChanged;
                    }
                }

                if (val == null)
                {
                    continue;
                }
                val.Instance = t;
                val.Property = property;
                val.IsReadOnly = !property.CanWrite;
                val.Value = val.Value;

                if (isNew)
                {
                    Values.Add(val);
                }
            }
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
#nullable enable
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Config))]
    [JsonSerializable(typeof(ConfigKey))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}