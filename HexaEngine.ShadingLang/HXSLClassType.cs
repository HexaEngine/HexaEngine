using Hexa.NET.Utilities;

namespace HexaEngine.ShadingLang
{
    public struct HXSLClassType : IFreeable
    {
        public HXSLType Base = new(HXSLTypeKind.Class);
        public UnsafeList<HXSLField> Fields = [];

        public HXSLClassType()
        {
        }

        public readonly StdWString Name => Base.Name;

        public void Release()
        {
            Base.ReleaseCore();
            foreach (var field in Fields)
            {
                field.Release();
            }
            Fields.Release();
        }
    }
}