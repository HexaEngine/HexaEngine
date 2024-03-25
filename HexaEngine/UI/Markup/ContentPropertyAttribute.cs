namespace HexaEngine.UI.Markup
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ContentPropertyAttribute : Attribute
    {
        public ContentPropertyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class LocalizabilityAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class UsableDuringInitializationAttribute : Attribute
    {
        public UsableDuringInitializationAttribute(bool usable)
        {
            Usable = usable;
        }

        public bool Usable { get; }
    }
}