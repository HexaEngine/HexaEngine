using Hexa.NET.Utilities;
using System.Runtime.CompilerServices;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLType : IFreeable
    {
        public HXSLSymbol Symbol;
        public HXSLTypeKind Kind;
        public HXSLNamespace* Namespace;
        public StdWString Name;

        public HXSLType(HXSLTypeKind kind)
        {
            Symbol = new(default, HXSLSymbolType.Type);
            Kind = kind;
        }

        public void Release()
        {
            switch (Kind)
            {
                case HXSLTypeKind.Unknown:
                    ReleaseCore();
                    break;

                case HXSLTypeKind.Primitive:
                    ((HXSLPrimitiveType*)Unsafe.AsPointer(ref this))->Release();
                    break;

                case HXSLTypeKind.Struct:
                    ((HXSLStructType*)Unsafe.AsPointer(ref this))->Release();
                    break;

                case HXSLTypeKind.Class:
                    ((HXSLClassType*)Unsafe.AsPointer(ref this))->Release();
                    break;

                default:
                    throw new Exception("Unknown type kind");
            }
        }

        public void ReleaseCore()
        {
            Name.Release();
            Namespace = null;
        }
    }
}