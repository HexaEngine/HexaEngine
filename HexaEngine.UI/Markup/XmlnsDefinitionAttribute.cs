namespace HexaEngine.UI.Markup
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public class XmlnsDefinitionAttribute : Attribute
    {
        public string XmlNamespace { get; }
        public string ClrNamespace { get; }

        public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
        {
            XmlNamespace = xmlNamespace;
            ClrNamespace = clrNamespace;
        }
    }
}