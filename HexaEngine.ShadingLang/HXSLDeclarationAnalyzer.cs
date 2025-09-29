using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe class HXSLDeclarationAnalyzer : HXSLAnalyzer
    {
        public override unsafe bool TryParse(HXSLModule module, ref HXSLParser parser, ref TokenStream stream, HXSLCompilation* compilation)
        {
            stream.PushState();
            var startingToken = stream.Current;

            HXSLModifierFlags flags = parser.ParseModifiers();

            if (!parser.TryParseType(out var type))
            {
                stream.PopState();
                return false;
            }

            if (!stream.TryGetIdentifier(out var name))
            {
                Free(type);
                stream.PopState();
                return false;
            }

            try
            {
                if (stream.TryGetDelimiter('('))
                {
                    var function = ParseFunction(ref parser, ref stream, compilation, flags, type, name);
                    function->Span = startingToken.Span.Merge(stream.LastToken.Span);
                    parser.CurrentNamespace->Functions.Add(function);
                    stream.PopState(false);
                    return true;
                }
                else if (stream.TryGetDelimiter(';'))
                {
                    var field = ParseField(ref parser, ref stream, compilation, flags, type, name);
                    field->Span = startingToken.Span.Merge(stream.LastToken.Span);
                    if (parser.ScopeType == ScopeType.Struct)
                    {
                        var structType = (HXSLStructType*)parser.ScopeContext.Userdata;
                        structType->Fields.Add(field);
                    }
                    else if (parser.ScopeType == ScopeType.Namespace)
                    {
                        parser.CurrentNamespace->Fields.Add(field);
                    }

                    stream.PopState(false);
                    return true;
                }
                else if (stream.TryGetDelimiter(':') && stream.TryGetIdentifier(out var fieldSemantic) && stream.TryGetDelimiter(';'))
                {
                    var field = ParseField(ref parser, ref stream, compilation, flags, type, name);
                    field->Semantic = fieldSemantic;
                    field->Span = startingToken.Span.Merge(stream.LastToken.Span);
                    if (parser.ScopeType == ScopeType.Struct)
                    {
                        var structType = (HXSLStructType*)parser.ScopeContext.Userdata;
                        structType->Fields.Add(field);
                    }
                    else if (parser.ScopeType == ScopeType.Namespace)
                    {
                        parser.CurrentNamespace->Fields.Add(field);
                    }

                    stream.PopState(false);
                    return true;
                }
                else
                {
                    Free(type);
                    stream.PopState();
                    return false;
                }
            }
            catch (Exception)
            {
                Free(type);
                stream.PopState();
                throw;
            }
        }

        private static unsafe HXSLFunction* ParseFunction(ref HXSLParser parser, ref TokenStream stream, HXSLCompilation* compilation, HXSLModifierFlags flags, HXSLUnresolvedType* type, TextSpan name)
        {
            HXSLFunction function = new()
            {
                Name = name,
                ReturnType = (HXSLType*)type,
                Flags = flags.ToFunctionFlags(),
                Span = default
            };

            parser.CurrentNamespace->Unresolved.Add(type);

            UnsafeList<Pointer<HXSLParameter>> parameters = [];

            try
            {
                bool firstParameter = true;
                while (!stream.TryGetDelimiter(')'))
                {
                    if (!firstParameter)
                    {
                        stream.ExpectDelimiter(',');
                    }
                    firstParameter = false;

                    parameters.Add(ParseParameter(ref parser, ref stream));
                }

                if (stream.TryGetDelimiter(':'))
                {
                    function.Semantic = stream.ExpectIdentifier();
                }

                function.ExpressionBody = ParseExpressionBody(name, ref parser, ref stream);
            }
            catch
            {
                foreach (HXSLParameter* param in parameters)
                {
                    Free(param);
                }
                parameters.Release();
                throw;
            }

            function.Parameters = parameters;

            return AllocT(function);
        }

        private static unsafe HXSLParameter* ParseParameter(ref HXSLParser parser, ref TokenStream stream)
        {
            var startingToken = stream.Current;
            HXSLParameter parameter = new()
            {
                Flags = parser.ParseParameterFlags()
            };

            var paramType = parser.ParseType();
            try
            {
                var paramName = stream.ExpectIdentifier();
                parameter.Type = (HXSLType*)paramType;
                parameter.Name = paramName;

                if (stream.TryGetDelimiter(':'))
                {
                    parameter.Semantic = stream.ExpectIdentifier();
                }

                parameter.Span = startingToken.Span.Merge(stream.Current.Span);

                parser.CurrentNamespace->Unresolved.Add(paramType);
                return AllocT(parameter);
            }
            catch (Exception)
            {
                Free(paramType);
                throw;
            }
        }

        private static HXSLExpression ParseExpressionBody(TextSpan name, ref HXSLParser parser, ref TokenStream stream)
        {
            var first = parser.EnterScope(name, ScopeType.Function, null);
            var last = parser.SkipScope();
            return new() { Span = first.Span.Merge(last.Span) };
        }

        private static unsafe HXSLField* ParseField(ref HXSLParser parser, ref TokenStream stream, HXSLCompilation* compilation, HXSLModifierFlags flags, HXSLUnresolvedType* type, TextSpan name)
        {
            HXSLField field = new()
            {
                Name = name,
                Type = (HXSLType*)type,
                Flags = flags.ToFieldFlags(),
            };

            parser.CurrentNamespace->Unresolved.Add(type);

            return AllocT(field);
        }
    }
}