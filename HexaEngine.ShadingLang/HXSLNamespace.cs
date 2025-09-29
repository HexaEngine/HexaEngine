using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLNamespace : IFreeable
    {
        public NamespaceDeclaration Namespace;
        public UnsafeList<UsingDeclaration> Usings;
        public UnsafeList<Pointer<HXSLStructType>> Structs;
        public UnsafeList<Pointer<HXSLClassType>> Classes;
        public UnsafeList<Pointer<HXSLFunction>> Functions;
        public UnsafeList<Pointer<HXSLField>> Fields;
        public UnsafeList<Pointer<HXSLUnresolvedType>> Unresolved;
        public UnsafeList<VariableReference> References;

        public void Release()
        {
            Usings.Release();
            foreach (HXSLStructType* @struct in Structs)
            {
                Free(@struct);
            }
            Structs.Release();
            foreach (HXSLClassType* @class in Classes)
            {
                Free(@class);
            }
            Classes.Release();
            foreach (HXSLFunction* @function in Functions)
            {
                Free(@function);
            }
            Functions.Release();
            foreach (HXSLField* @field in Fields)
            {
                Free(@field);
            }
            Fields.Release();
            Unresolved.Release();
            References.Release();
        }

        public readonly bool TryStructType(TextSpan identifer, out HXSLStructType* type)
        {
            var span = identifer.AsSpan();
            foreach (HXSLStructType* item in Structs)
            {
                if (item->Name.AsSpan().SequenceEqual(span))
                {
                    type = item;
                    return true;
                }
            }
            type = null;
            return false;
        }
    }
}