using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.LexicalAnalysis;
using HexaEngine.ShadingLang.Text;
using System.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe class HXSLModule
    {
        public string Version { get; set; }

        public List<HXSLInclude> Imports { get; } = [];

        public List<HXSLProperty> Properties { get; } = [];

        public List<HXSLShader> Shaders { get; } = [];

        public List<HXSLPass> Passes { get; } = [];

        public HXSLModule()
        {
        }

        public HXSLShader? FindShader(ReadOnlySpan<char> name)
        {
            foreach (var shader in Shaders)
            {
                if (name.SequenceEqual(shader.Name))
                {
                    return shader;
                }
            }
            return null;
        }

        public HXSLProperty? FindProperty(ReadOnlySpan<char> name)
        {
            foreach (var property in Properties)
            {
                if (name.SequenceEqual(property.Name))
                {
                    return property;
                }
            }
            return null;
        }

        private UnsafeDictionary<StdWString, HXSLPrimitiveType> PrimitiveTypesCache;

        public unsafe string CompileShaderToHLSL(HXSLShader shader)
        {
            fixed (char* pText = shader.Code)
            {
                LexerState state = new(pText, shader.Code.Length);
                TokenStream stream = new(state, LexerConfig.HLSLConfig);

                HXSLCompilation* compilation = AllocT<HXSLCompilation>();
                HXSLParser parser = new(ref stream, compilation);
                while (parser.TryAdvance())
                {
                    while (HXSLAnalyzerRegistry.TryParse(this, ref parser, ref stream, compilation)) ;
                }

                Populate();

                foreach (HXSLNamespace* ns in compilation->Namespaces)
                {
                    for (int i = ns->Unresolved.Count - 1; i >= 0; i--)
                    {
                        HXSLUnresolvedType* type = ns->Unresolved[i];
                        if (TryGetPrimitiveType(type->Token.Span, out var primitiveType))
                        {
                            *(HXSLPrimitiveType*)type = primitiveType;
                            ns->Unresolved.RemoveAt(i);
                            continue;
                        }

                        if (ns->TryStructType(type->Token.Span, out var structType))
                        {
                            *(HXSLStructType*)type = *structType;
                            ns->Unresolved.RemoveAt(i);
                            continue;
                        }
                    }
                }

                compilation->Release();
            }

            return shader.Code;
        }

        public bool TryGetPrimitiveType(TextSpan identifier, out HXSLPrimitiveType type)
        {
            StdWString str = new(identifier.Text + identifier.Start, identifier.Length, identifier.Length);

            if (PrimitiveTypesCache.TryGetValue(str, out type))
            {
                return true;
            }

            return false;
        }

        private void Populate()
        {
            AddPrim(HXSLPrimitiveKind.Void, HXSLPrimitiveClass.Scalar, 1, 1);
            for (HXSLPrimitiveKind kind = HXSLPrimitiveKind.Bool; kind <= HXSLPrimitiveKind.Min16Uint; kind++)
            {
                // add scalar
                AddPrim(kind, HXSLPrimitiveClass.Scalar, 1, 1);

                // add vectors
                for (uint n = 2; n <= 4; ++n)
                {
                    AddPrim(kind, HXSLPrimitiveClass.Vector, n, 1);
                }

                // add matrices
                for (uint r = 1; r <= 4; ++r)
                {
                    for (uint c = 1; c <= 4; ++c)
                    {
                        AddPrim(kind, HXSLPrimitiveClass.Matrix, r, c);
                    }
                }
            }
        }

        private void AddPrim(HXSLPrimitiveKind kind, HXSLPrimitiveClass primitiveClass, uint rows, uint columns)
        {
            StdWString name = new();
            switch (kind)
            {
                case HXSLPrimitiveKind.Void:
                    name.Append("void".AsSpan());
                    break;

                case HXSLPrimitiveKind.Bool:
                    name.Append("bool".AsSpan());
                    break;

                case HXSLPrimitiveKind.Int:
                    name.Append("int".AsSpan());
                    break;

                case HXSLPrimitiveKind.Float:
                    name.Append("float".AsSpan());
                    break;

                case HXSLPrimitiveKind.Uint:
                    name.Append("uint".AsSpan());
                    break;

                case HXSLPrimitiveKind.Double:
                    name.Append("double".AsSpan());
                    break;

                case HXSLPrimitiveKind.Min8Float:
                    name.Append("min8float".AsSpan());
                    break;

                case HXSLPrimitiveKind.Min10Float:
                    name.Append("min10float".AsSpan());
                    break;

                case HXSLPrimitiveKind.Min16Float:
                    name.Append("min16float".AsSpan());
                    break;

                case HXSLPrimitiveKind.Min12Int:
                    name.Append("min12int".AsSpan());
                    break;

                case HXSLPrimitiveKind.Min16Int:
                    name.Append("min16int".AsSpan());
                    break;

                case HXSLPrimitiveKind.Min16Uint:
                    name.Append("min16uint".AsSpan());
                    break;
            }

            switch (primitiveClass)
            {
                case HXSLPrimitiveClass.Scalar:
                    break;

                case HXSLPrimitiveClass.Vector:
                    name.Append((char)('0' + rows));
                    break;

                case HXSLPrimitiveClass.Matrix:
                    name.Append((char)('0' + rows));
                    name.Append('x');
                    name.Append((char)('0' + columns));
                    break;
            }

            PrimitiveTypesCache.Add(name, new HXSLPrimitiveType(name, primitiveClass, kind, rows, columns));
        }
    }
}