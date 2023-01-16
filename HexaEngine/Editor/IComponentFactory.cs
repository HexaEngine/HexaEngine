namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Core.Scenes;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public interface IComponentFactory
    {
        public string Name { get; }

        PropertyInfo[] Properties { get; }
        Type Type { get; }

        IComponent Create();

        bool IsType(IComponent other);
    }

    public class ComponentFactory : IComponentFactory
    {
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        private readonly Type type;

        public ComponentFactory([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
        {
            this.type = type;
            Type = type;
            Name = type.Name;
            Properties = type.GetProperties();
        }

        public Type Type { get; }

        public string Name { get; }

        public PropertyInfo[] Properties { get; }

        public IComponent Create()
        {
            return (IComponent?)Activator.CreateInstance(type) ?? throw new();
        }

        public bool IsType(IComponent other)
        {
            return other.GetType() == type;
        }
    }

    public class ComponentFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IComponentFactory where T : IComponent, new()
    {
        public ComponentFactory()
        {
            var type = typeof(T);
            Type = type;
            Name = type.Name;
            Properties = type.GetProperties();
        }

        public Type Type { get; }

        public string Name { get; }

        public PropertyInfo[] Properties { get; }

        public IComponent Create()
        {
            return new T();
        }

        public bool IsType(IComponent other)
        {
            return other is T;
        }
    }
}