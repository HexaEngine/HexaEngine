namespace HexaEngine.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Numerics;
    using System.Reflection;

    public class ConfigKey
    {
        public ConfigKey(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public List<ConfigValue> Values { get; set; } = new();

        public List<ConfigKey> Keys { get; set; } = new();

        public void Sort()
        {
            Values.Sort(AZConfigValueComparer.Default);
            Keys.Sort(AZConfigKeyComparer.Default);
        }

        public ConfigKey AddKey(string path)
        {
            ConfigKey? key = Find(Keys, path);
            if (key != null)
                throw new("Key already exists");
            key = new(path);
            Keys.Add(key);
            return key;
        }

        public bool RemoveKey(string path)
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                ConfigKey key = Keys[i];
                if (key.Name == path)
                {
                    Keys.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

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

        public bool TryGetKey(string path, [NotNullWhen(true)] out ConfigKey? key)
        {
            key = Find(Keys, path);
            return key != null;
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
                {
                    return true;
                }
            }
            return false;
        }

        public static ConfigValue? Find(List<ConfigValue> values, string key)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var val = values[i];
                if (val.Name == key)
                {
                    return val;
                }
            }
            return null;
        }

        public void AddValue(string key, DataType type, string? value, bool isReadOnly)
        {
            ConfigValue configValue = new(key, type, value, isReadOnly);
            configValue.DefaultValue = value;
            Values.Add(configValue);
        }

        public bool RemoveValue(string key)
        {
            for (int i = 0; i < Values.Count; i++)
            {
                var val = Values[i];
                if (val.Name == key)
                {
                    Values.RemoveAt(i);
                    return true;
                }
            }
            return false;
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

        public ConfigKey GenerateSubKeyAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t, string name)
        {
            ConfigKey? key = Find(Keys, name);
            bool isNew = key == null;
            key ??= new(name);
            key.InitAuto(t);
            if (isNew)
            {
                Keys.Add(key);
            }
            return key;
        }

        public static ConfigKey? Find(List<ConfigKey> keys, string name)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Name == name)
                {
                    return keys[i];
                }
            }
            return null;
        }

        #region EventHandlers

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

        #endregion EventHandlers

        public void InitAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t)
        {
#nullable disable

            List<ConfigValue> usedValues = new();
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
                usedValues.Add(val);
                if (isNew)
                {
                    Values.Add(val);
                }
            }

            Values.Clear();
            Values.AddRange(usedValues);

#nullable enable
        }
    }
}