using Hexa.NET.Utilities;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLStructType : IFreeable
    {
        public HXSLType Base = new(HXSLTypeKind.Struct);
        public HXSLAccessModifier AccessModifier;
        public UnsafeList<Pointer<HXSLField>> Fields = [];

        public HXSLStructType()
        {
        }

        public readonly StdWString Name => Base.Name;

        public void Release()
        {
            Base.ReleaseCore();
            foreach (HXSLField* field in Fields)
            {
                Free(field);
            }
            Fields.Release();
        }
    }
}