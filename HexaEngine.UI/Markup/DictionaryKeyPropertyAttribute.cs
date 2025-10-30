namespace HexaEngine.UI.Markup
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DictionaryKeyPropertyAttribute : Attribute
    {
        public DictionaryKeyPropertyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}