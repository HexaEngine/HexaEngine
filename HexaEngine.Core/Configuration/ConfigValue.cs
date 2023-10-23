namespace HexaEngine.Core
{
    using HexaEngine.Core.Input;
    using System.Globalization;
    using System.Numerics;
    using System.Reflection;
    using System.Text.Json.Serialization;

    public class ConfigValue
    {
        private readonly object _lock = new();

        private object? instance;
        private PropertyInfo? property;
        private Type? enumType;

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

        [JsonIgnore]
        public Type? EnumType { get => enumType; internal set => enumType = value; }

        public DataType DataType { get; set; }

        public string Name { get; set; }

        public string? Value
        {
            get
            {
                lock (_lock)
                {
                    return value;
                }
            }

            set
            {
                lock (_lock)
                {
                    this.value = value;
                    ValueChanged?.Invoke(this, value);
                }
            }
        }

        public bool IsReadOnly { get; set; }

        [JsonIgnore]
        public string? DefaultValue { get => defaultValue; set => defaultValue = value; }

        [JsonIgnore]
        public object SyncObject => _lock;

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

        public T GetEnum<T>() where T : struct, Enum
        {
            if (Value == null)
            {
                return default;
            }

            return Enum.Parse<T>(Value);
        }

        public object GetEnum(Type type)
        {
            if (Value == null)
            {
#nullable disable
                return Activator.CreateInstance(type);
#nullable enable
            }

            return Enum.Parse(type, Value);
        }

        public IEnumerable<Key> GetKeys()
        {
            if (Value == null)
            {
                yield break;
            }

            string[] keys = Value.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keys.Length; i++)
            {
                yield return (Key)Enum.Parse(typeof(Key), keys[i]);
            }
        }

        public T Get<T>(Func<string?, T> convert)
        {
            return convert(Value);
        }
    }
}