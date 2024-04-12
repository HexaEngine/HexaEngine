namespace HexaEngine.Core.Configuration
{
    using HexaEngine.Core;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Numerics;
    using System.Reflection;

    /// <summary>
    /// Represents a configuration key that can contain values and subkeys.
    /// </summary>
    public class ConfigKey
    {
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigKey"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the configuration key.</param>
        public ConfigKey(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the configuration key.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a list of configuration values associated with this key.
        /// </summary>
        public List<ConfigValue> Values { get; set; } = new();

        /// <summary>
        /// Gets or sets a list of subkeys contained within this configuration key.
        /// </summary>
        public List<ConfigKey> Keys { get; set; } = new();

        /// <summary>
        /// Gets an object used for synchronization to protect concurrent access to the key.
        /// </summary>
        [JsonIgnore]
        public object SyncObject => _lock;

        /// <summary>
        /// Sorts the configuration values and subkeys of this key.
        /// </summary>
        public void Sort()
        {
            lock (_lock)
            {
                Values.Sort(AZConfigValueComparer.Instance);
                Keys.Sort(AZConfigKeyComparer.Instance);
            }
        }

        /// <summary>
        /// Sorts the configuration values and subkeys of this key using custom comparers.
        /// </summary>
        /// <param name="keyComparer">The comparer to use for sorting subkeys.</param>
        /// <param name="valueComparer">The comparer to use for sorting values.</param>
        public void Sort(IComparer<ConfigKey> keyComparer, IComparer<ConfigValue> valueComparer)
        {
            lock (_lock)
            {
                Values.Sort(valueComparer);
                Keys.Sort(keyComparer);
            }
        }

        /// <summary>
        /// Adds a subkey to this configuration key with the specified name.
        /// </summary>
        /// <param name="path">The name of the subkey to add.</param>
        /// <returns>The newly added subkey.</returns>
        public ConfigKey AddKey(string path)
        {
            lock (_lock)
            {
                ConfigKey? key = Find(Keys, path);
                if (key != null)
                {
                    throw new("Key already exists");
                }

                key = new(path);
                Keys.Add(key);
                return key;
            }
        }

        /// <summary>
        /// Tries to add a new configuration key with the specified name.
        /// </summary>
        /// <param name="path">The path of the key to add.</param>
        /// <param name="key">When this method returns, contains the added <see cref="ConfigKey"/> if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the key was added successfully; otherwise, <c>false</c>.</returns>
        public bool TryAddKey(string path, [NotNullWhen(true)] out ConfigKey? key)
        {
            lock (_lock)
            {
                key = Find(Keys, path);
                if (key != null)
                {
                    return false;
                }

                key = new(path);
                Keys.Add(key);
                return true;
            }
        }

        /// <summary>
        /// Removes a subkey from this configuration key with the specified name.
        /// </summary>
        /// <param name="path">The name of the subkey to remove.</param>
        /// <returns><c>true</c> if the subkey was removed; otherwise, <c>false</c>.</returns>
        public bool RemoveKey(string path)
        {
            lock (_lock)
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
        }

        /// <summary>
        /// Gets or creates a subkey within this configuration key with the specified name.
        /// </summary>
        /// <param name="path">The name of the subkey to get or create.</param>
        /// <returns>The subkey with the specified name, creating it if it doesn't exist.</returns>
        public ConfigKey GetOrCreateKey(string path)
        {
            lock (_lock)
            {
                ConfigKey? key = Find(Keys, path);
                if (key is null)
                {
                    key = new ConfigKey(path);
                    Keys.Add(key);
                }
                return key;
            }
        }

        /// <summary>
        /// Tries to get a subkey by name within this configuration key.
        /// </summary>
        /// <param name="path">The name of the subkey to find.</param>
        /// <param name="key">When this method returns, contains the subkey if found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the subkey was found; otherwise, <c>false</c>.</returns>
        public bool TryGetKey(string path, [NotNullWhen(true)] out ConfigKey? key)
        {
            lock (_lock)
            {
                key = Find(Keys, path);
                return key != null;
            }
        }

        /// <summary>
        /// Tries to get the value associated with a key within this configuration key.
        /// </summary>
        /// <param name="key">The name of the key to find.</param>
        /// <param name="value">When this method returns, contains the value if found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the key was found and has a value; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            lock (_lock)
            {
                value = Find(Values, key)?.Value;
                return value != null;
            }
        }

        /// <summary>
        /// Tries to get the configuration value associated with a key within this configuration key.
        /// </summary>
        /// <param name="key">The name of the key to find.</param>
        /// <param name="value">When this method returns, contains the configuration value if found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the key was found and has a configuration value; otherwise, <c>false</c>.</returns>
        public bool TryGetKeyValue(string key, [NotNullWhen(true)] out ConfigValue? value)
        {
            lock (_lock)
            {
                value = Find(Values, key);
                return value != null;
            }
        }

        /// <summary>
        /// Tries to get a configuration value by key, creating it if it doesn't exist.
        /// </summary>
        /// <param name="key">The name of the key to find or create.</param>
        /// <param name="defaultValue">The default value to set if the key doesn't exist.</param>
        /// <param name="type">The data type of the configuration value.</param>
        /// <param name="isReadOnly">A value indicating whether the configuration value is read-only.</param>
        /// <param name="value">When this method returns, contains the configuration value.</param>
        public void TryGetOrAddKeyValue(string key, string? defaultValue, DataType type, bool isReadOnly, out ConfigValue value)
        {
            lock (_lock)
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
        }

        /// <summary>
        /// Tries to get a value by key, creating it if it doesn't exist.
        /// </summary>
        /// <param name="key">The name of the key to find or create.</param>
        /// <param name="defaultValue">The default value to set if the key doesn't exist.</param>
        /// <param name="type">The data type of the value.</param>
        /// <param name="isReadOnly">A value indicating whether the value is read-only.</param>
        /// <param name="value">When this method returns, contains the value.</param>
        /// <returns><c>true</c> if the value was found or created; otherwise, <c>false</c>.</returns>
        public bool TryGetOrAddValue(string key, string? defaultValue, DataType type, bool isReadOnly, [NotNullWhen(true)] out string? value)
        {
            lock (_lock)
            {
                ConfigValue? val = Find(Values, key);
                if (val is null)
                {
                    val = new(key, type, defaultValue, isReadOnly);
                    Values.Add(val);
                }
                value = val.Value;
                return value != null;
            }
        }

        /// <summary>
        /// Determines whether this configuration key contains a subkey with the specified name.
        /// </summary>
        /// <param name="key">The name of the subkey to check for.</param>
        /// <returns><c>true</c> if a subkey with the specified name exists; otherwise, <c>false</c></returns>.
        public bool ContainsKey(string key)
        {
            lock (_lock)
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
        }

        /// <summary>
        /// Finds a configuration value in the provided list of values with the specified key.
        /// </summary>
        /// <param name="values">The list of configuration values to search in.</param>
        /// <param name="key">The key to look for in the list of values.</param>
        /// <returns>The configuration value with the specified key, or <c>null</c> if not found.</returns>
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

        /// <summary>
        /// Adds a configuration value to the list of values in this configuration key.
        /// </summary>
        /// <param name="key">The name of the configuration value to add.</param>
        /// <param name="type">The data type of the configuration value.</param>
        /// <param name="value">The value to set for the configuration value.</param>
        /// <param name="isReadOnly">A value indicating whether the configuration value is read-only.</param>
        public void AddValue(string key, DataType type, string? value, bool isReadOnly)
        {
            lock (_lock)
            {
                ConfigValue configValue = new(key, type, value, isReadOnly);
                configValue.DefaultValue = value;
                Values.Add(configValue);
            }
        }

        /// <summary>
        /// Adds a configuration value to the list of values in this configuration key with an associated enum type.
        /// </summary>
        /// <param name="key">The name of the configuration value to add.</param>
        /// <param name="type">The data type of the configuration value.</param>
        /// <param name="value">The value to set for the configuration value.</param>
        /// <param name="isReadOnly">A value indicating whether the configuration value is read-only.</param>
        /// <param name="enumType">The enum type associated with the configuration value.</param>
        /// <returns>The newly created config value key</returns>
        public ConfigValue AddValue(string key, DataType type, string? value, bool isReadOnly, Type? enumType)
        {
            lock (_lock)
            {
                ConfigValue configValue = new(key, type, value, isReadOnly);
                configValue.DefaultValue = value;
                configValue.EnumType = enumType;
                Values.Add(configValue);
                return configValue;
            }
        }

        /// <summary>
        /// Removes a configuration value from this configuration key with the specified name.
        /// </summary>
        /// <param name="key">The name of the configuration value to remove.</param>
        /// <returns><c>true</c> if the configuration value was removed; otherwise, <c>false</c>.</returns>
        public bool RemoveValue(string key)
        {
            lock (_lock)
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
        }

        /// <summary>
        /// Gets the integer value associated with the specified key. If the key does not exist,
        /// it adds a new integer value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default integer value to add if the key doesn't exist.</param>
        /// <returns>The integer value associated with the key or the default value if the key doesn't exist.</returns>
        public int GetOrAddValue(string key, int defaultValue)
        {
            lock (_lock)
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
        }

        /// <summary>
        /// Gets the floating-point value associated with the specified key. If the key does not exist,
        /// it adds a new floating-point value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default floating-point value to add if the key doesn't exist.</param>
        /// <returns>The floating-point value associated with the key or the default value if the key doesn't exist.</returns>
        public float GetOrAddValue(string key, float defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetValue(key, out string? value))
                {
                    AddValue(key, DataType.Float, defaultValue.ToString(null, CultureInfo.InvariantCulture), false);
                    return defaultValue;
                }
                if (value == null)
                {
                    return defaultValue;
                }

                return float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the double floating-point value associated with the specified key. If the key does not exist,
        /// it adds a new floating-point value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default double floating-point value to add if the key doesn't exist.</param>
        /// <returns>The double floating-point value associated with the key or the default value if the key doesn't exist.</returns>
        public double GetOrAddValue(string key, double defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetValue(key, out string? value))
                {
                    AddValue(key, DataType.Double, defaultValue.ToString(null, CultureInfo.InvariantCulture), false);
                    return defaultValue;
                }
                if (value == null)
                {
                    return defaultValue;
                }

                return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the boolean value associated with the specified key. If the key does not exist,
        /// it adds a new boolean value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default boolean value to add if the key doesn't exist.</param>
        /// <returns>The boolean value associated with the key or the default value if the key doesn't exist.</returns>
        public bool GetOrAddValue(string key, bool defaultValue)
        {
            lock (_lock)
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
        }

        /// <summary>
        /// Gets a Vector2 value associated with the specified key. If the key does not exist,
        /// it adds a new Vector2 value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default Vector2 value to add if the key doesn't exist.</param>
        /// <returns>The Vector2 value associated with the key or the default value if the key doesn't exist.</returns>
        public Vector2 GetOrAddValue(string key, Vector2 defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    AddValue(key, DataType.Float2, defaultValue.ToString(null, CultureInfo.InvariantCulture), false);
                    return defaultValue;
                }

                if (value == null)
                {
                    return defaultValue;
                }

                return value.GetVector2();
            }
        }

        /// <summary>
        /// Gets a Vector3 value associated with the specified key. If the key does not exist,
        /// it adds a new Vector3 value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default Vector3 value to add if the key doesn't exist.</param>
        /// <returns>The Vector3 value associated with the key or the default value if the key doesn't exist.</returns>
        public Vector3 GetOrAddValue(string key, Vector3 defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    AddValue(key, DataType.Float3, defaultValue.ToString(null, CultureInfo.InvariantCulture), false);
                    return defaultValue;
                }

                if (value == null)
                {
                    return defaultValue;
                }

                return value.GetVector3();
            }
        }

        /// <summary>
        /// Gets a Vector4 value associated with the specified key. If the key does not exist,
        /// it adds a new Vector4 value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default Vector4 value to add if the key doesn't exist.</param>
        /// <returns>The Vector4 value associated with the key or the default value if the key doesn't exist.</returns>
        public Vector4 GetOrAddValue(string key, Vector4 defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    AddValue(key, DataType.Float4, defaultValue.ToString(null, CultureInfo.InvariantCulture), false);
                    return defaultValue;
                }

                if (value == null)
                {
                    return defaultValue;
                }

                return value.GetVector4();
            }
        }

        /// <summary>
        /// Gets an enum value associated with the specified key. If the key does not exist,
        /// it adds a new enum value with the provided default value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default enum value to add if the key doesn't exist.</param>
        /// <returns>The enum value associated with the key or the default value if the key doesn't exist.</returns>
        public T GetOrAddValue<T>(string key, T defaultValue) where T : struct, Enum
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    AddValue(key, DataType.Enum, defaultValue.ToString(), false, typeof(T));
                    return defaultValue;
                }

                value.EnumType ??= typeof(T);

                var val = value.Value;

                if (val == null)
                {
                    return defaultValue;
                }

                return Enum.Parse<T>(val);
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey(string key, int defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Int32, defaultValue.ToString(), false, typeof(int));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey(string key, float defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Float, defaultValue.ToString(), false, typeof(float));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey(string key, double defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Double, defaultValue.ToString(), false, typeof(double));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey(string key, bool defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Bool, defaultValue.ToString(), false, typeof(bool));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey(string key, Vector2 defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Float2, defaultValue.ToString(), false, typeof(Vector2));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey(string key, Vector3 defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Float3, defaultValue.ToString(), false, typeof(Vector3));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey(string key, Vector4 defaultValue)
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Float4, defaultValue.ToString(), false, typeof(Vector4));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets an value associated with the specified key. If the key does not exist,
        /// it adds a new value with the provided default value.
        /// </summary>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <returns>The value key associated with the key or the default value if the key doesn't exist.</returns>
        public ConfigValue GetOrAddValueKey<T>(string key, T defaultValue) where T : struct, Enum
        {
            lock (_lock)
            {
                if (!TryGetKeyValue(key, out var value))
                {
                    return AddValue(key, DataType.Enum, defaultValue.ToString(), false, typeof(T));
                }

                return value;
            }
        }

        /// <summary>
        /// Gets a value associated with the specified key and applies custom conversion functions.
        /// If the key does not exist, it adds a new value with the provided default value.
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve or add.</typeparam>
        /// <param name="key">The key to look up or add.</param>
        /// <param name="defaultValue">The default value to add if the key doesn't exist.</param>
        /// <param name="type">The data type of the configuration value.</param>
        /// <param name="convert">A function to convert a string to the target value type.</param>
        /// <param name="convertBack">A function to convert the target value type back to a string.</param>
        /// <returns>The value associated with the key or the default value if the key doesn't exist.</returns>
        public T GetOrAddValue<T>(string key, T defaultValue, DataType type, Func<string?, T> convert, Func<T, string?> convertBack)
        {
            lock (_lock)
            {
                if (!TryGetValue(key, out string? value))
                {
                    AddValue(key, type, convertBack(defaultValue), false);
                    return defaultValue;
                }

                return convert(value);
            }
        }

        /// <summary>
        /// Sets a string value associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The string value to set.</param>
        public void SetValue(string key, string? value)
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value;
                }
            }
        }

        /// <summary>
        /// Sets a double value associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The double value to set.</param>
        public void SetValue(string key, double value)
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value.ToString(null, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Sets a float value associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The float value to set.</param>
        public void SetValue(string key, float value)
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value.ToString(null, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Sets a Vector2 value associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The Vector2 value to set.</param>
        public void SetValue(string key, Vector2 value)
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value.ToString(null, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Sets a Vector3 value associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The Vector3 value to set.</param>
        public void SetValue(string key, Vector3 value)
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value.ToString(null, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Sets a Vector4 value associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The Vector4 value to set.</param>
        public void SetValue(string key, Vector4 value)
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value.ToString(null, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Sets a value associated with the specified key. This overload is used for nullable value types.
        /// </summary>
        /// <typeparam name="T">The type of value to set.</typeparam>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue<T>(string key, T? value)
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value?.ToString();
                }
            }
        }

        /// <summary>
        /// Sets an enum value associated with the specified key. This overload is used for nullable enum value types.
        /// </summary>
        /// <typeparam name="T">The enum type of the value to set.</typeparam>
        /// <param name="key">The key for which to set the value.</param>
        /// <param name="value">The enum value to set.</param>
        public void SetValue<T>(string key, T? value) where T : struct, Enum
        {
            lock (_lock)
            {
                if (TryGetKeyValue(key, out var kvalue))
                {
                    kvalue.Value = value?.ToString();
                    kvalue.EnumType ??= typeof(T);
                }
            }
        }

        /// <summary>
        /// Generates a subkey automatically based on a provided object's public properties and the subkey name.
        /// If a subkey with the same name already exists, it will be updated; otherwise, a new subkey will be added.
        /// </summary>
        /// <typeparam name="T">The type of the provided object, which determines the subkey's structure.</typeparam>
        /// <param name="t">The object used to initialize the subkey's structure.</param>
        /// <param name="name">The name of the subkey.</param>
        /// <returns>The generated or updated subkey.</returns>
        public ConfigKey GenerateSubKeyAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t, string name)
        {
            lock (_lock)
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
        }

        /// <summary>
        /// Searches for a subkey with the specified name in the provided list of subkeys.
        /// </summary>
        /// <param name="keys">The list of subkeys to search within.</param>
        /// <param name="name">The name of the subkey to find.</param>
        /// <returns>The subkey with the specified name, or null if it is not found.</returns>
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
                property.SetValue(t, configValue.GetFloat());
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
                property.SetValue(t, configValue.GetDouble());
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

        private static void EnumValueChanged(object? sender, string? e)
        {
            if (sender is not ConfigValue configValue)
            {
                return;
            }
            var property = configValue.Property;
            var type = configValue.EnumType;
            var t = configValue.Instance;
#nullable disable
            if (e != null)
            {
                property.SetValue(t, configValue.GetEnum(type));
            }
            else
            {
                property.SetValue(t, Activator.CreateInstance(type));
            }
#nullable enable
        }

        #endregion EventHandlers

        /// <summary>
        /// Automatically initializes and populates ConfigValue objects based on the public properties of the specified object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object for which ConfigValue objects will be initialized.</typeparam>
        /// <param name="t">The object for which ConfigValue objects will be initialized.</param>
        /// <param name="allowReadOnly">Specifies whether to include read-only properties in the initialization (default is false).</param>
        public void InitAuto<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T t, bool allowReadOnly = false)
        {
#nullable disable
            lock (_lock)
            {
                List<ConfigValue> usedValues = new();
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                for (int i = properties.Length - 1; i >= 0; i--)
                {
                    PropertyInfo property = properties[i];

                    if (!property.CanRead)
                    {
                        continue;
                    }

                    if (!allowReadOnly && !property.CanWrite)
                    {
                        continue;
                    }

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

                    if (property.PropertyType.IsEnum)
                    {
                        val ??= new(t, property, DataType.Enum, property.GetValue(t)?.ToString(), !property.CanWrite);
                        val.EnumType = property.PropertyType;
                        if (property.CanWrite)
                        {
                            val.ValueChanged += EnumValueChanged;
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
            }
#nullable enable
        }
    }
}