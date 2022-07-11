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

        public string Name { get; }
        public EditorPropertyMode Mode { get; }
    }

    public enum EditorPropertyMode
    {
        Default,
        Colorpicker,
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