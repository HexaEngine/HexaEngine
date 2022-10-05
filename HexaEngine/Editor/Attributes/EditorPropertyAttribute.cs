namespace HexaEngine.Editor.Attributes
{
    using HexaEngine.Scenes;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorPropertyAttribute : Attribute
    {
        public EditorPropertyAttribute(string name, EditorPropertyMode mode = EditorPropertyMode.Default)
        {
            Name = name;
            Mode = mode;
        }

        public EditorPropertyAttribute(string name, object min, object max, EditorPropertyMode mode = EditorPropertyMode.Slider)
        {
            Name = name;
            Mode = mode;
            Min = min;
            Max = max;
        }

        public EditorPropertyAttribute(string name, Type type, EditorPropertyMode mode = EditorPropertyMode.TypeSelector)
        {
            Name = name;
            Type = type;
            Mode = mode;
        }

        public string Name { get; }

        public EditorPropertyMode Mode { get; }

        public object? Min { get; }

        public object? Max { get; }

        public Type? Type { get; }
    }

    public enum EditorPropertyMode
    {
        Default,
        Colorpicker,
        Slider,
        SliderAngle,
        TypeSelector,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorNodeAttribute : Attribute
    {
        public EditorNodeAttribute(string name, Type type, Func<SceneNode> constructor, Func<SceneNode, bool> isType)
        {
            Name = name;
            Type = type;
            Constructor = constructor;
            IsType = isType;
        }

        public string Name { get; }

        public Type Type { get; }

        public Func<SceneNode> Constructor { get; }

        public Func<SceneNode, bool> IsType { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorNodeAttribute<T> : EditorNodeAttribute where T : SceneNode, new()
    {
        public EditorNodeAttribute(string name) : base(name, typeof(T), () => new T(), (SceneNode other) => other is T)
        {
        }
    }
}