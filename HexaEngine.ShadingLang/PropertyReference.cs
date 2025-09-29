using HexaEngine.Materials.Generator;

namespace HexaEngine.ShadingLang
{
    public struct PropertyReference
    {
        public HXSLProperty Property;
        public string LocalName;
        public SType Type;
        public int Offset;
        public int Size;

        public PropertyReference(HXSLProperty property, string localName)
        {
            Property = property;
            LocalName = localName;
            Type = property.ToSType();
        }
    }
}