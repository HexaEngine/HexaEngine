namespace HexaEngine.ShadingLang
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class HXSLNameAttribute : Attribute
    {
        public string Name;

        public HXSLNameAttribute(string name)
        {
            Name = name;
        }
    }
}