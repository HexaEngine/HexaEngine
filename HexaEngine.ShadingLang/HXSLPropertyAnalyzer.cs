using Hexa.NET.Utilities;
using Hexa.NET.Utilities.Text;
using HexaEngine.ShadingLang.Text;
using System.Text;

namespace HexaEngine.ShadingLang
{
    public class HXSLPropertyAnalyzer : HXSLAnalyzer
    {
        private const int alignment = 16;
        private StringBuilder sbCb = new();

        public HXSLPropertyAnalyzer()
        {
            sbCb.AppendLine("cbuffer Properties");
            sbCb.AppendLine("{");
        }

        private static unsafe VariableReference ParseVariableReference(ref TokenStream stream)
        {
            VariableReference reference = default;
            stream.ExpectIdentifier(out reference.PropertyName);
            stream.ExpectIdentifier(out reference.ShaderPropertyName);
            var last = stream.ExpectDelimiter(';');
            int start = reference.PropertyName.Start - 1;
            int end = last.Span.Start + 1;
            reference.Span = new TextSpan(reference.PropertyName.Text, start, (end - start));
            return reference;
        }

        public override unsafe bool TryParse(HXSLModule module, ref HXSLParser parser, ref TokenStream stream, HXSLCompilation* compilation)
        {
            if (stream.TryGetDelimiter('@'))
            {
                if (parser.ScopeLevel != parser.NamespaceScope) throw new Exception("Variable references must be at the global/namespace scope.");
                var reference = ParseVariableReference(ref stream);
                parser.CurrentNamespace->References.Add(reference);
                return true;
            }

            return false;
        }

        /*
        public unsafe void Compile(HXSLModule module, StringBuilder sb, ref int offset, HXSLCompilation* compilation)
        {
            int cbOffset = 0;
            int paddCounter = 0;
            StdWString str = new();
            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);
            foreach (var reference in compilation->References)
            {
                var property = module.FindProperty(reference.PropertyName) ?? throw new InvalidOperationException($"Property '{reference.PropertyName}' not found.");
                PropertyReference propRef = new(property, reference.ShaderPropertyName.ToString());

                if (!propRef.Type.IsTexture)
                {
                    var size = propRef.Type.SizeOf();

                    int currentPack = cbOffset % alignment;
                    if (currentPack + size > alignment)
                    {
                        cbOffset = cbOffset - currentPack + alignment;
                        int rem = alignment - currentPack;
                        int paddCount = 0;
                        string paddType;
                        if (rem % 4 == 0)
                        {
                            paddType = "float";
                            paddCount = rem / 4;
                        }
                        else if (rem % 2 == 0)
                        {
                            paddType = "half";
                            paddCount = rem / 2;
                        }
                        else
                        {
                            throw new Exception("Cannot determine padding type.");
                        }

                        for (int x = 0; x < paddCount; x++, paddCounter++)
                        {
                            sbCb.AppendLine($"\t{paddType} _padd{paddCounter};");
                        }
                    }

                    sbCb.AppendLine($"\t{propRef.Type} {propRef.LocalName}");

                    propRef.Offset = cbOffset;
                    propRef.Size = size;

                    cbOffset += size;

                    sb.Remove(reference.Span.Start + offset, reference.Span.Length);
                    offset -= reference.Span.Length;
                }
            }
            if (compilation->References.Count > 0)
            {
                sbCb.AppendLine("};");
                sb.Insert(0, sbCb);
            }
        }*/
    }
}