namespace HexaEngine.Editor.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorComponentAttribute : Attribute
    {
        public EditorComponentAttribute(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public EditorComponentAttribute(Type type, string name, bool isHidden) : this(type, name)
        {
            IsHidden = isHidden;
        }

        public EditorComponentAttribute(Type type, string name, bool isHidden, bool isInternal) : this(type, name)
        {
            IsHidden = isHidden;
            IsInternal = isInternal;
        }

        public EditorComponentAttribute(Type type, string name, params Type[] allowedTypes) : this(type, name)
        {
            AllowedTypes = allowedTypes;
        }

        public EditorComponentAttribute(Type type, string name, Type[] allowedTypes, Type[] disallowedTypes) : this(type, name)
        {
            AllowedTypes = allowedTypes;
            DisallowedTypes = disallowedTypes;
        }

        public Type Type { get; }

        public string Name { get; }

        public bool IsHidden { get; }

        public bool IsInternal { get; }

        public Type[] AllowedTypes { get; }

        public Type[] DisallowedTypes { get; }
    }
}