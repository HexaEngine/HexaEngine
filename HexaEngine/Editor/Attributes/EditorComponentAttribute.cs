namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Editor.Properties;
    using HexaEngine.Objects;
    using System;
    using System.Diagnostics.CodeAnalysis;

    public delegate IComponent ComponentConstructor();

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorComponentAttribute : Attribute
    {
        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name)
        {
            Type = type;
            Constructor = () => ((IComponent?)Activator.CreateInstance(type)) ?? throw new();
            Name = name;
            IsType = x => type.IsAssignableFrom(x.GetType());
            //Editor = new PropertyEditor(type);
        }

        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, bool isHidden) : this(type, name)
        {
            IsHidden = isHidden;
        }

        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, bool isHidden, bool isInternal) : this(type, name)
        {
            IsHidden = isHidden;
            IsInternal = isInternal;
        }

        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, params Type[] allowedTypes) : this(type, name)
        {
            AllowedTypes = allowedTypes;
        }

        public EditorComponentAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, string name, Type[] allowedTypes, Type[] disallowedTypes) : this(type, name)
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

        public IObjectEditor Editor { get; }
    }

#if GenericAttributes

    public class EditorComponentAttribute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : EditorComponentAttribute where T : class, IComponent, new()
    {
        public EditorComponentAttribute(string name) : base(typeof(T), name)
        {
        }

        public EditorComponentAttribute(string name, bool isHidden) : base(typeof(T), name, isHidden)
        {
        }

        public EditorComponentAttribute(string name, params Type[] allowedTypes) : base(typeof(T), name, allowedTypes)
        {
        }

        public EditorComponentAttribute(string name, bool isHidden, bool isInternal) : base(typeof(T), name, isHidden, isInternal)
        {
        }

        public EditorComponentAttribute(string name, Type[] allowedTypes, Type[] disallowedTypes) : base(typeof(T), name, allowedTypes, disallowedTypes)
        {
        }
    }

#endif
}