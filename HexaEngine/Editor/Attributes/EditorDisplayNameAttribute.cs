namespace HexaEngine.Editor.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorDisplayNameAttribute : Attribute
    {
        public EditorDisplayNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}