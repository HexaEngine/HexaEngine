namespace HexaEngine.Core.Editor.Attributes
{
    using HexaEngine.Core.Scenes;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorPropertyAttribute : Attribute
    {
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

        public string Name { get; }

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

    public class EditorPropertyTypeSelectorAttribute<T> : EditorPropertyAttribute
    {
        public EditorPropertyTypeSelectorAttribute(string name, params Type[] types)
            : base(name,
                  typeof(T),
                  types,
                  types.Select(x => x.Name).ToArray(),
                  EditorPropertyMode.TypeSelector)
        {
        }
    }

    public enum EditorPropertyMode
    {
        Default,
        Enum,
        Colorpicker,
        Slider,
        SliderAngle,
        TypeSelector,
        Filepicker,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorNodeAttribute : Attribute
    {
        public EditorNodeAttribute(string name, Type type, Func<GameObject> constructor, Func<GameObject, bool> isType)
        {
            Name = name;
            Type = type;
            Constructor = constructor;
            IsType = isType;
        }

        public string Name { get; }

        public Type Type { get; }

        public Func<GameObject> Constructor { get; }

        public Func<GameObject, bool> IsType { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorNodeAttribute<T> : EditorNodeAttribute where T : GameObject, new()
    {
        public EditorNodeAttribute(string name) : base(name, typeof(T), () => new T(), (other) => other is T)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EditorButtonAttribute : Attribute
    {
        public EditorButtonAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}