namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Objects;
    using System;
    using System.Diagnostics.CodeAnalysis;

    public delegate IComponent ComponentConstructor();

#if GenericAttributes
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
#endif

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorComponentAttribute : Attribute
    {
        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
        {
            Type = type;
            Constructor = () => ((IComponent?)Activator.CreateInstance(type)) ?? throw new();
            Name = type.Name;
            IsType = x => type.IsAssignableFrom(x.GetType());
        }

        public EditorComponentAttribute(IComponentFactory factory, bool isHidden) : this(factory)
        {
            IsHidden = isHidden;
        }

        public EditorComponentAttribute(IComponentFactory factory, bool isHidden, bool isInternal) : this(factory)
        {
            IsHidden = isHidden;
            IsInternal = isInternal;
        }

        public EditorComponentAttribute(IComponentFactory factory, params Type[] allowedTypes)
        {
            Type = factory.Type;
            Constructor = factory.Create;
            Name = factory.Name;
            IsType = factory.IsType;
            AllowedTypes = allowedTypes;
        }

        public EditorComponentAttribute(IComponentFactory factory, Type[] allowedTypes, Type[] disallowedTypes) : this(factory)
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

#if GenericAttributes
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
#endif
}