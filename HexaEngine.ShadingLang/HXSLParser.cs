using Hexa.NET.Utilities;
using HexaEngine.ShadingLang.LexicalAnalysis;
using HexaEngine.ShadingLang.LexicalAnalysis.HXSL;
using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe ref struct HXSLParser
    {
        public ref TokenStream Stream;
        public int ScopeLevel;
        public int NamespaceScope;
        public HXSLCompilation* Compilation;
        public HXSLNamespace* CurrentNamespace;
        public ScopeContext ScopeContext;
        public UnsafeStack<ScopeContext> ScopeStack;
        public static readonly StdWString Unknown = new("Unknown");

        public HXSLParser(ref TokenStream stream, HXSLCompilation* compilation)
        {
            Stream = ref stream;
            Compilation = compilation;
        }

        public readonly ScopeType ScopeType => ScopeContext.Type;

        public readonly TextSpan ScopeName => ScopeContext.Name;

        public bool TryAdvance()
        {
            if (!Stream.TryAdvance())
            {
                return false;
            }

            while (true)
            {
                if (Stream.TryGetKeyword(HXSLKeyword.Namespace))
                {
                    if (ScopeLevel != 0) throw new Exception("Namespaces must be at the global scope.");
                    if (CurrentNamespace != null) throw new Exception("Only one namespace can be declared in the current scope.");
                    CurrentNamespace = Compilation->AddNamespace(NamespaceDeclaration.FromStream(ref this, out var scoped));
                    NamespaceScope = ScopeLevel;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Using))
                {
                    if (!IsInGlobalOrNamespaceScope) throw new Exception("Usings must be at the global or namespace scope.");
                    var declaration = UsingDeclaration.FromStream(ref Stream);
                    if (CurrentNamespace != null)
                    {
                        CurrentNamespace->Usings.PushBack(declaration);
                    }
                    else
                    {
                        Compilation->Usings.PushBack(declaration);
                    }
                }
                else if (Stream.TryGetDelimiter('{'))
                {
                    EnterScopeInternal(Unknown.AsTextSpan(), ScopeType.Unknown, null);
                    return false;
                }
                else if (Stream.TryGetDelimiter('}'))
                {
                    ExitScopeInternal();
                    return false;
                }
                else
                {
                    break;
                }

                if (Stream.IsEndOfTokens)
                {
                    return false;
                }
            }

            if (CurrentNamespace == null)
            {
                throw new InvalidOperationException("Expected namespace.");
            }

            return true;
        }

        public bool TryEnterScope(TextSpan name, ScopeType type, void* userdata)
        {
            if (Stream.TryGetDelimiter('{'))
            {
                EnterScopeInternal(name, type, userdata);
                return true;
            }
            return false;
        }

        public Token EnterScope(TextSpan name, ScopeType type, void* userdata)
        {
            var token = Stream.ExpectDelimiter('{');
            EnterScopeInternal(name, type, userdata);
            return token;
        }

        public Token SkipScope()
        {
            int targetScope = ScopeLevel - 1;
            while (TryAdvance())
            {
                if (ScopeLevel == targetScope)
                {
                    return Stream.LastToken;
                }
            }

            if (ScopeLevel == targetScope)
            {
                return Stream.LastToken;
            }

            throw new Exception("Unexpected end of tokens.");
        }

        public bool IterateScope()
        {
            if (Stream.TryGetDelimiter('}'))
            {
                ExitScopeInternal();
                return false;
            }
            return true;
        }

        private void EnterScopeInternal(TextSpan name, ScopeType type, void* userdata)
        {
            ScopeStack.Push(ScopeContext);
            ScopeContext = new(name, type, userdata);
            ScopeLevel++;
        }

        private void ExitScopeInternal()
        {
            ScopeLevel--;
            if (ScopeLevel < 0)
            {
                throw new InvalidOperationException("Scope level cannot be smaller than 0.");
            }

            ScopeContext = ScopeStack.Pop();

            if (ScopeLevel < NamespaceScope)
            {
                NamespaceScope = 0;
                CurrentNamespace = null;
            }
        }

        public readonly bool IsInNamespaceScope(bool strict = false) => strict ? ScopeLevel == NamespaceScope : ScopeLevel >= NamespaceScope;

        public readonly bool IsInGlobalOrNamespaceScope => ScopeLevel == NamespaceScope || ScopeLevel == 0;

        public HXSLUnresolvedType* ParseType()
        {
            if (TryParseType(out var type))
            {
                return type;
            }

            throw new Exception("Unexpected token.");
        }

        public bool TryParseType(out HXSLUnresolvedType* type)
        {
            if (Stream.TryGetKeyword(HXSLKeyword.Void))
            {
                type = AllocT(new HXSLUnresolvedType() { Base = new(HXSLTypeKind.Void), Token = Stream.LastToken });
                return true;
            }
            else if (Stream.TryGetKeyword(HXSLKeyword.Bool) &&
                Stream.TryGetKeyword(HXSLKeyword.Uint) &&
                Stream.TryGetKeyword(HXSLKeyword.Int) &&
                Stream.TryGetKeyword(HXSLKeyword.Float) &&
                Stream.TryGetKeyword(HXSLKeyword.Double) &&
                Stream.TryGetKeyword(HXSLKeyword.Matrix) &&
                Stream.TryGetKeyword(HXSLKeyword.Min16float) &&
                Stream.TryGetKeyword(HXSLKeyword.Min10float) &&
                Stream.TryGetKeyword(HXSLKeyword.Min16int) &&
                Stream.TryGetKeyword(HXSLKeyword.Min12int) &&
                Stream.TryGetKeyword(HXSLKeyword.Min16uint))
            {
                type = AllocT(new HXSLUnresolvedType() { Base = new(HXSLTypeKind.Unknown), Token = Stream.LastToken });
                return true;
            }
            else if (Stream.TryGetIdentifier(out var _))
            {
                type = AllocT(new HXSLUnresolvedType() { Base = new(HXSLTypeKind.Unknown), Token = Stream.LastToken });
                return true;
            }

            type = null;
            return false;
        }

        public HXSLAccessModifier ParseAccessModifier()
        {
            HXSLAccessModifier modifier = HXSLAccessModifier.Private;
            while (true)
            {
                if (Stream.TryGetKeyword(HXSLKeyword.Public))
                {
                    modifier = HXSLAccessModifier.Public;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Internal))
                {
                    modifier = HXSLAccessModifier.Internal;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Private))
                {
                    modifier = HXSLAccessModifier.Private;
                }
                else
                {
                    break;
                }
            }

            return modifier;
        }

        public HXSLModifierFlags ParseModifiers()
        {
            HXSLModifierFlags flags = 0;
            while (true)
            {
                if (Stream.TryGetKeyword(HXSLKeyword.Public))
                {
                    flags |= HXSLModifierFlags.Public;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Private))
                {
                    flags |= HXSLModifierFlags.Private;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Inline))
                {
                    flags |= HXSLModifierFlags.Inline;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Static))
                {
                    flags |= HXSLModifierFlags.Static;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Nointerpolation))
                {
                    flags |= HXSLModifierFlags.Nointerpolation;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Shared))
                {
                    flags |= HXSLModifierFlags.Shared;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Groupshared))
                {
                    flags |= HXSLModifierFlags.GroupShared;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Uniform))
                {
                    flags |= HXSLModifierFlags.Uniform;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Volatile))
                {
                    flags |= HXSLModifierFlags.Volatile;
                }
                else
                {
                    break;
                }
            }
            return flags;
        }

        public HXSLParameterFlags ParseParameterFlags()
        {
            HXSLParameterFlags flags = 0;
            while (true)
            {
                if (Stream.TryGetKeyword(HXSLKeyword.In))
                {
                    flags |= HXSLParameterFlags.In;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Out))
                {
                    flags |= HXSLParameterFlags.Out;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Inout))
                {
                    flags |= HXSLParameterFlags.InOut;
                }
                else if (Stream.TryGetKeyword(HXSLKeyword.Uniform))
                {
                    flags |= HXSLParameterFlags.Uniform;
                }
                else
                {
                    break;
                }
            }

            return flags;
        }

        public void Release()
        {
            ScopeStack.Release();
        }
    }
}