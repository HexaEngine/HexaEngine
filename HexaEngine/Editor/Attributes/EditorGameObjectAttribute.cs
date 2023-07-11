namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Core.Scenes;
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorGameObjectAttribute : Attribute
    {
        public EditorGameObjectAttribute(string name, Type type, Func<GameObject> constructor, Func<GameObject, bool> isType)
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
    public class EditorGameObjectAttribute<T> : EditorGameObjectAttribute where T : GameObject, new()
    {
        public EditorGameObjectAttribute(string name) : base(name, typeof(T), () => new T(), (other) => other is T)
        {
        }
    }
}