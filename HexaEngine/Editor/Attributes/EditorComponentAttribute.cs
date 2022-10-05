namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Objects;
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorComponentAttribute : Attribute
    {
        public EditorComponentAttribute(Type type, Func<IComponent> constructor, string name, Func<IComponent, bool> isType)
        {
            Type = type;
            Constructor = constructor;
            Name = name;
            IsType = isType;
        }

        public EditorComponentAttribute(Type type, Func<IComponent> constructor, Func<IComponent, bool> isType, string name, bool isHidden) : this(type, constructor, isType, name)
        {
            IsHidden = isHidden;
        }

        public EditorComponentAttribute(Type type, Func<IComponent> constructor, Func<IComponent, bool> isType, string name, bool isHidden, bool isInternal) : this(type, constructor, isType, name)
        {
            IsHidden = isHidden;
            IsInternal = isInternal;
        }

        public EditorComponentAttribute(Type type, Func<IComponent> constructor, Func<IComponent, bool> isType, string name, params Type[] allowedTypes)
        {
            Type = type;
            Constructor = constructor;
            Name = name;
            IsType = isType;
            AllowedTypes = allowedTypes;
        }

        public EditorComponentAttribute(Type type, Func<IComponent> constructor, Func<IComponent, bool> isType, string name, Type[] allowedTypes, Type[] disallowedTypes) : this(type, constructor, isType, name)
        {
            AllowedTypes = allowedTypes;
            DisallowedTypes = disallowedTypes;
        }

        public Func<IComponent> Constructor { get; }

        public Func<IComponent, bool> IsType { get; }

        public Type Type { get; }

        public string Name { get; }

        public bool IsHidden { get; }

        public bool IsInternal { get; }

        public Type[]? AllowedTypes { get; }

        public Type[]? DisallowedTypes { get; }
    }

    public class EditorComponentAttribute<T> : EditorComponentAttribute where T : class, IComponent, new()
    {
        public EditorComponentAttribute(string name) : base(typeof(T), () => new T(), (IComponent other) => other is T, name)
        {
        }

        public EditorComponentAttribute(string name, bool isHidden) : base(typeof(T), () => new T(), (IComponent other) => other is T, name, isHidden)
        {
        }

        public EditorComponentAttribute(string name, params Type[] allowedTypes) : base(typeof(T), () => new T(), (IComponent other) => other is T, name, allowedTypes)
        {
        }

        public EditorComponentAttribute(string name, bool isHidden, bool isInternal) : base(typeof(T), () => new T(), (IComponent other) => other is T, name, isHidden, isInternal)
        {
        }

        public EditorComponentAttribute(string name, Type[] allowedTypes, Type[] disallowedTypes) : base(typeof(T), () => new T(), (IComponent other) => other is T, name, allowedTypes, disallowedTypes)
        {
        }
    }
}