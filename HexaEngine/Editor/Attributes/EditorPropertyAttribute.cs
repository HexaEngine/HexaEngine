namespace HexaEngine.Editor.Attributes
{
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
        public EditorNodeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}