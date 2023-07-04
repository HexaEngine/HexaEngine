namespace HexaEngine.Core.Editor.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorPropertyAttribute : Attribute
    {
        public EditorPropertyAttribute(EditorPropertyMode mode = EditorPropertyMode.Default)
        {
            Name = string.Empty;
            Mode = mode;
        }

        public EditorPropertyAttribute(string name, EditorPropertyMode mode = EditorPropertyMode.Default)
        {
            Name = name;
            Mode = mode;
        }

        public EditorPropertyAttribute(string name, string? startingPath, string? filter = null, string? relativeTo = null, EditorPropertyMode mode = EditorPropertyMode.Filepicker)
        {
            Name = name;
            StartingPath = startingPath;
            Filter = filter;
            RelativeTo = relativeTo;
            Mode = mode;
        }

        public EditorPropertyAttribute(string name, object[] enumValues, string[] enumNames, EditorPropertyMode mode = EditorPropertyMode.Enum)
        {
            Name = name;
            EnumValues = enumValues;
            EnumNames = enumNames;
            Mode = mode;
        }

        public EditorPropertyAttribute(string name, object min, object max, EditorPropertyMode mode = EditorPropertyMode.Slider)
        {
            Name = name;
            Mode = mode;
            Min = min;
            Max = max;
        }

        public EditorPropertyAttribute(string name, Type target, Type[] types, string[] typeNames, EditorPropertyMode mode = EditorPropertyMode.TypeSelector)
        {
            Name = name;
            TargetType = target;
            Types = types;
            TypeNames = typeNames;
            Mode = mode;
        }

        public string Name { get; set; }

        public EditorPropertyMode Mode { get; }

        public object? Min { get; }

        public object? Max { get; }

        public Type? TargetType { get; }

        public Type[]? Types { get; }

        public string[]? TypeNames { get; }

        public object[]? EnumValues { get; }

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
    }

    public class EditorPropertyAttribute<T> : EditorPropertyAttribute where T : struct, Enum
    {
        public EditorPropertyAttribute(string name) :
            base(name,
                Enum.GetValues<T>().Cast<object>().ToArray(),
                Enum.GetNames<T>(),
                EditorPropertyMode.Enum)
        {
        }
    }
}