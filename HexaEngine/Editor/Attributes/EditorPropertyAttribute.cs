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

        public EditorPropertyAttribute(string name, EditorPropertyMode mode, object min, object max)
        {
            Name = name;
            Mode = mode;
            Min = min;
            Max = max;
        }

        public string Name { get; }

        public EditorPropertyMode Mode { get; }

        public object Min { get; }

        public object Max { get; }
    }

    public enum EditorPropertyMode
    {
        Default,
        Colorpicker,
        Slider,
        SliderAngle,
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