using Hexa.NET.Utilities;

namespace HexaEngine.ShadingLang
{
    public unsafe struct HXSLCompilation : IFreeable
    {
        public UnsafeList<UsingDeclaration> Usings;
        public UnsafeList<Pointer<HXSLNamespace>> Namespaces;
        public UnsafeList<Pointer<HXSLSymbol>> Symbols;
        public UnsafeList<HXSLSymbol> ExternalSymbols;

        public HXSLNamespace* AddNamespace(NamespaceDeclaration declaration)
        {
            HXSLNamespace* ns = AllocT<HXSLNamespace>();
            ns->Namespace = declaration;
            int index = Namespaces.Size;
            Namespaces.PushBack(ns);
            return ns;
        }

        public void Release()
        {
            Usings.Release();
            foreach (HXSLNamespace* ns in Namespaces)
            {
                Free(ns);
            }
            Namespaces.Release();
            Symbols.Release();
            ExternalSymbols.Release();
        }
    }
}