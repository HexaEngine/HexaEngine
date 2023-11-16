namespace HexaEngine.Editor.Attributes
{
    using System;
    using System.Diagnostics.CodeAnalysis;

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

    public enum EditorPropertyConditionMode
    {
        Visible,
        Enable,
        ReadOnly,
    }

    public delegate bool EditorPropertyCondition(object instance);

    public delegate bool EditorPropertyCondition<T>(T instance);

    /// <summary>
    /// Specifies a condition for the visibility, enabling, or read-only state of an editor property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class EditorPropertyConditionAttribute : Attribute
    {
        public EditorPropertyConditionAttribute(string methodName, EditorPropertyConditionMode mode = EditorPropertyConditionMode.Visible)
        {
            MethodName = methodName;
            Mode = mode;
        }

        /// <summary>
        /// Gets the condition function.
        /// </summary>
        public EditorPropertyCondition Condition { get; set; }

        public string MethodName { get; }

        /// <summary>
        /// Gets the mode specifying how the condition affects the property.
        /// </summary>
        public EditorPropertyConditionMode Mode { get; }
    }

    /// <summary>
    /// Specifies a condition for the visibility, enabling, or read-only state of an editor property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class EditorPropertyConditionAttribute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> : EditorPropertyConditionAttribute
    {
        public EditorPropertyConditionAttribute(string methodName, EditorPropertyConditionMode mode = EditorPropertyConditionMode.Visible) : base(methodName, mode)
        {
            var method = typeof(T).GetMethod(methodName);
            if (method == null)
            {
                throw new InvalidOperationException($"Method ({methodName}) was not found.");
            }
            GenericCondition = method.CreateDelegate<EditorPropertyCondition<T>>();
            Condition = ConditionMethod;
        }

        public EditorPropertyCondition<T> GenericCondition { get; set; }

        private bool ConditionMethod(object instance)
        {
            return GenericCondition((T)instance);
        }
    }
}