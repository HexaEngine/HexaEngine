using Hexa.NET.Utilities;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLPrimitiveType : IFreeable
    {
        public HXSLType Base = new(HXSLTypeKind.Primitive);
        public HXSLPrimitiveClass Class;
        public HXSLPrimitiveKind Kind;

        public uint Rows;
        public uint Columns;

        public HXSLPrimitiveType(StdWString name, HXSLPrimitiveClass primitiveClass, HXSLPrimitiveKind primitiveKind, uint rows, uint columns)
        {
            Base.Name = name;
            Class = primitiveClass;
            Kind = primitiveKind;
            Rows = rows;
            Columns = columns;
        }

        public readonly StdWString Name => Base.Name;

        public void Release()
        {
            Base.ReleaseCore();
        }

        public static implicit operator HXSLType(HXSLPrimitiveType v) => v.Base;
    }
}