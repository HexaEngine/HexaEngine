namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Core.Assets;
    using System;

    /// <summary>
    /// Represents an attribute that provides additional information about a property for the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified mode.
        /// </summary>
        /// <param name="mode">The editor property mode.</param>
        public EditorPropertyAttribute(EditorPropertyMode mode = EditorPropertyMode.Default)
        {
            Name = string.Empty;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name and mode.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="mode">The editor property mode.</param>
        public EditorPropertyAttribute(string name, EditorPropertyMode mode = EditorPropertyMode.Default)
        {
            Name = name;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name and mode.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="mode">The editor property mode.</param>
        public EditorPropertyAttribute(string name, object? defaultValue, EditorPropertyMode mode = EditorPropertyMode.Default)
        {
            Name = name;
            DefaultValue = defaultValue;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name and mode.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="assetType">The asset type.</param>
        /// <param name="mode">The editor property mode.</param>
        public EditorPropertyAttribute(string name, AssetType assetType, EditorPropertyMode mode = EditorPropertyMode.Default)
        {
            Name = name;
            AssetType = assetType;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name, enum values, enum names, and mode for enum property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="enumValues">The values of the enum property.</param>
        /// <param name="enumNames">The names of the enum values.</param>
        /// <param name="mode">The editor property mode for enum property.</param>
        public EditorPropertyAttribute(string name, object[] enumValues, string[] enumNames, EditorPropertyMode mode = EditorPropertyMode.Enum)
        {
            Name = name;
            EnumValues = enumValues;
            EnumNames = enumNames;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name, enum values, enum names, and mode for enum property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="enumValues">The values of the enum property.</param>
        /// <param name="enumNames">The names of the enum values.</param>
        /// <param name="mode">The editor property mode for enum property.</param>
        public EditorPropertyAttribute(string name, object? defaultValue, object[] enumValues, string[] enumNames, EditorPropertyMode mode = EditorPropertyMode.Enum)
        {
            Name = name;
            DefaultValue = defaultValue;
            EnumValues = enumValues;
            EnumNames = enumNames;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name, min, max, and mode for slider property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="min">The minimum value for the slider property.</param>
        /// <param name="max">The maximum value for the slider property.</param>
        /// <param name="mode">The editor property mode for slider property.</param>
        public EditorPropertyAttribute(string name, object min, object max, EditorPropertyMode mode = EditorPropertyMode.Slider)
        {
            Name = name;
            Mode = mode;
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name, min, max, and mode for slider property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="min">The minimum value for the slider property.</param>
        /// <param name="max">The maximum value for the slider property.</param>
        /// <param name="mode">The editor property mode for slider property.</param>
        public EditorPropertyAttribute(string name, object? defaultValue, object min, object max, EditorPropertyMode mode = EditorPropertyMode.Slider)
        {
            Name = name;
            DefaultValue = defaultValue;
            Mode = mode;
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name, target type, types, type names, and mode for type selector property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="target">The target type for the type selector property.</param>
        /// <param name="types">The types available in the type selector property.</param>
        /// <param name="typeNames">The names of the available types.</param>
        /// <param name="mode">The editor property mode for type selector property.</param>
        public EditorPropertyAttribute(string name, Type target, Type[] types, string[] typeNames, EditorPropertyMode mode = EditorPropertyMode.TypeSelector)
        {
            Name = name;
            TargetType = target;
            Types = types;
            TypeNames = typeNames;
            Mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute"/> class with the specified name, target type, types, type names, and mode for type selector property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="target">The target type for the type selector property.</param>
        /// <param name="types">The types available in the type selector property.</param>
        /// <param name="typeNames">The names of the available types.</param>
        /// <param name="mode">The editor property mode for type selector property.</param>
        public EditorPropertyAttribute(string name, object? defaultValue, Type target, Type[] types, string[] typeNames, EditorPropertyMode mode = EditorPropertyMode.TypeSelector)
        {
            Name = name;
            DefaultValue = defaultValue;
            TargetType = target;
            Types = types;
            TypeNames = typeNames;
            Mode = mode;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Gets or sets the default value of the property.
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Gets the editor property mode.
        /// </summary>
        public EditorPropertyMode Mode { get; }

        /// <summary>
        /// Gets the minimum value for the slider property.
        /// </summary>
        public object? Min { get; }

        /// <summary>
        /// Gets the maximum value for the slider property.
        /// </summary>
        public object? Max { get; }

        /// <summary>
        /// Gets the target type for the type selector property.
        /// </summary>
        public Type? TargetType { get; }

        /// <summary>
        /// Gets the types available in the type selector property.
        /// </summary>
        public Type[]? Types { get; }

        /// <summary>
        /// Gets the names of the available types.
        /// </summary>
        public string[]? TypeNames { get; }

        /// <summary>
        /// Gets the values of the enum property.
        /// </summary>
        public object[]? EnumValues { get; }

        /// <summary>
        /// Gets the names of the enum values.
        /// </summary>
        public string[]? EnumNames { get; }

        /// <summary>
        /// Filename filter for file picker.
        /// </summary>
        public string? Filter { get; }

        /// <summary>
        /// Sets the starting path.
        /// </summary>
        public string? StartingPath { get; }

        /// <summary>
        /// Returns the path relative to this value
        /// </summary>
        public string? RelativeTo { get; }

        /// <summary>
        /// Returns the asset type.
        /// </summary>
        public AssetType AssetType { get; }
    }

    /// <summary>
    /// Specifies additional attributes for an editor property with a strongly typed enum value.
    /// </summary>
    /// <typeparam name="T">The type of the strongly typed enum.</typeparam>
    public class EditorPropertyAttribute<T> : EditorPropertyAttribute where T : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute{T}"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public EditorPropertyAttribute() :
            base(string.Empty,
                Enum.GetValues<T>().Cast<object>().ToArray(),
                Enum.GetNames<T>(),
                EditorPropertyMode.Enum)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute{T}"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public EditorPropertyAttribute(string name) :
            base(name,
                Enum.GetValues<T>().Cast<object>().ToArray(),
                Enum.GetNames<T>(),
                EditorPropertyMode.Enum)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPropertyAttribute{T}"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        public EditorPropertyAttribute(string name, T defaultValue) :
            base(name,
                defaultValue,
                Enum.GetValues<T>().Cast<object>().ToArray(),
                Enum.GetNames<T>(),
                EditorPropertyMode.Enum)
        {
        }
    }
}