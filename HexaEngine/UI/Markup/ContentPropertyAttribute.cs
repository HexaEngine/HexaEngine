namespace HexaEngine.UI.Markup
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ContentPropertyAttribute : Attribute
    {
        public ContentPropertyAttribute(string property)
        {
            Property = property;
        }

        public string Property { get; set; }
    }
}