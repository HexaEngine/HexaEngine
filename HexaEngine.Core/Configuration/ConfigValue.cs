namespace HexaEngine.Core
{
    using HexaEngine.Core.Input;
    using System.Globalization;
    using System.Numerics;
    using System.Reflection;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a configuration value that can hold various data types.
    /// </summary>
    public class ConfigValue
    {
        private readonly object _lock = new();

        private object? instance;
        private PropertyInfo? property;
        private Type? enumType;

        private string? value;
        private string? defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigValue"/> class.
        /// </summary>
        /// <param name="name">The name of the configuration value.</param>
        /// <param name="dataType">The data type of the configuration value.</param>
        /// <param name="value">The value of the configuration value.</param>
        /// <param name="isReadOnly">A value indicating whether the configuration value is read-only.</param>
        [JsonConstructor]
        public ConfigValue(string name, DataType dataType, string? value, bool isReadOnly)
        {
            Name = name;
            DataType = dataType;
            Value = value;
            IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigValue"/> class associated with an object instance and property information.
        /// </summary>
        /// <param name="instanceObject">The object instance with which this configuration value is associated.</param>
        /// <param name="propertyInfo">The property information of the associated property.</param>
        /// <param name="dataType">The data type of the configuration value.</param>
        /// <param name="value">The value of the configuration value.</param>
        /// <param name="isReadOnly">A value indicating whether the configuration value is read-only.</param>
        public ConfigValue(object instanceObject, PropertyInfo propertyInfo, DataType dataType, string? value, bool isReadOnly)
        {
            instance = instanceObject;
            property = propertyInfo;
            Name = propertyInfo.Name;
            DataType = dataType;
            Value = value;
            IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// Gets or sets the instance associated with this configuration value.
        /// </summary>
        [JsonIgnore]
        public object? Instance { get => instance; internal set => instance = value; }

        /// <summary>
        /// Gets or sets the property associated with this configuration value.
        /// </summary>
        [JsonIgnore]
        public PropertyInfo? Property { get => property; internal set => property = value; }

        /// <summary>
        /// Gets or sets the type of the enumeration associated with this configuration value.
        /// </summary>
        [JsonIgnore]
        public Type? EnumType { get => enumType; internal set => enumType = value; }

        /// <summary>
        /// Gets or sets the data type of this configuration value.
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Gets or sets the name of this configuration value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of this configuration value.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether this configuration value is read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the default value of this configuration value.
        /// </summary>
        [JsonIgnore]
        public string? DefaultValue { get => defaultValue; set => defaultValue = value; }

        /// <summary>
        /// Gets the synchronization object used for thread safety.
        /// </summary>
        [JsonIgnore]
        public object SyncObject => _lock;

        /// <summary>
        /// Occurs when the value of this configuration changes.
        /// </summary>
        public event Action<ConfigValue, string?>? ValueChanged;

        /// <summary>
        /// Sets the value of this configuration to its default value.
        /// </summary>
        public void SetToDefault()
        {
            Value = defaultValue;
        }

        /// <summary>
        /// Converts the value to an unsigned 8-bit integer.
        /// </summary>
        public byte GetUInt8()
        {
            if (Value == null)
            {
                return default;
            }

            return byte.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a signed 8-bit integer.
        /// </summary>
        public sbyte GetInt8()
        {
            if (Value == null)
            {
                return default;
            }

            return sbyte.Parse(Value);
        }

        /// <summary>
        /// Converts the value to an unsigned 16-bit integer.
        /// </summary>
        public ushort GetUInt16()
        {
            if (Value == null)
            {
                return default;
            }

            return ushort.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a signed 16-bit integer.
        /// </summary>
        public short GetInt16()
        {
            if (Value == null)
            {
                return default;
            }

            return short.Parse(Value);
        }

        /// <summary>
        /// Converts the value to an unsigned 32-bit integer.
        /// </summary>
        public uint GetUInt32()
        {
            if (Value == null)
            {
                return default;
            }

            return uint.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a signed 32-bit integer.
        /// </summary>
        public int GetInt32()
        {
            if (Value == null)
            {
                return default;
            }

            return int.Parse(Value);
        }

        /// <summary>
        /// Converts the value to an unsigned 64-bit integer.
        /// </summary>
        public ulong GetUInt64()
        {
            if (Value == null)
            {
                return default;
            }

            return ulong.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a signed 64-bit integer.
        /// </summary>
        public long GetInt64()
        {
            if (Value == null)
            {
                return default;
            }

            return long.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a single-precision floating-point number.
        /// </summary>
        public float GetFloat()
        {
            if (Value == null)
            {
                return default;
            }

            return float.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a double-precision floating-point number.
        /// </summary>
        public double GetDouble()
        {
            if (Value == null)
            {
                return default;
            }

            return double.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a boolean.
        /// </summary>
        public bool GetBool()
        {
            if (Value == null)
            {
                return default;
            }

            return bool.Parse(Value);
        }

        /// <summary>
        /// Converts the value to a 2D vector of single-precision floating-point numbers.
        /// </summary>
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

        /// <summary>
        /// Converts the value to a 3D vector of single-precision floating-point numbers.
        /// </summary>
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

        /// <summary>
        /// Converts the value to a 4D vector of single-precision floating-point numbers.
        /// </summary>
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

        /// <summary>
        /// Converts the value to an enumeration of the specified type.
        /// </summary>
        public T GetEnum<T>() where T : struct, Enum
        {
            if (Value == null)
            {
                return default;
            }

            return Enum.Parse<T>(Value);
        }

        /// <summary>
        /// Converts the value to an enumeration of the specified type.
        /// </summary>
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

        /// <summary>
        /// Converts the value to a collection of key values.
        /// </summary>
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

        /// <summary>
        /// Converts the value to the specified data type.
        /// </summary>
        public T Get<T>(Func<string?, T> convert)
        {
            return convert(Value);
        }
    }
}