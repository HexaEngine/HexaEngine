namespace HexaEngine.Editor.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EditorButtonAttribute : Attribute
    {
        public EditorButtonAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}