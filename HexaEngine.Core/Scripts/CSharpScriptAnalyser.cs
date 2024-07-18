namespace HexaEngine.Core.Scripts
{
    using HexaEngine.Core.Debugging;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public static class CSharpScriptAnalyser
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(CSharpScriptAnalyser));

        /// <summary>
        /// This must match the actual class in "HexaEngine.Scripts.ScriptBehaviour"
        /// </summary>
        public const string ScriptIdentifier = "ScriptBehaviour";

        /// <summary>
        /// This must match the actual interface in "HexaEngine.Scripts.IScriptBehaviour"
        /// </summary>
        public const string ScriptIdentifierInterface = "IScriptBehaviour";

        public static string? FindScript(string? filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            string code = File.ReadAllText(filePath);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var syntaxRoot = syntaxTree.GetRoot();

            var namespaceDeclaration = syntaxRoot.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();

            if (namespaceDeclaration == null)
            {
                return null;
            }

            string? name = null;
            bool found = false;
            foreach (var classDeclaration in syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (FindScriptInner(namespaceDeclaration, classDeclaration, out var className))
                {
                    if (!found)
                    {
                        name = className;
                    }
                    else
                    {
                        Logger.Warn($"Found multiple scripts in file '{filePath}', '{name}' will be used");
                    }
                    found = true;
                }
            }

            return name;
        }

        private static bool FindScriptInner(BaseNamespaceDeclarationSyntax namespaceDeclaration, ClassDeclarationSyntax classDeclaration, [MaybeNullWhen(false)] out string? fullName)
        {
            foreach (BaseTypeSyntax baseType in classDeclaration.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>())
            {
                if (baseType.Type is IdentifierNameSyntax typeSyntax)
                {
                    if (typeSyntax.Identifier.Text == ScriptIdentifier || typeSyntax.Identifier.Text == ScriptIdentifierInterface)
                    {
                        fullName = $"{namespaceDeclaration.Name}.{classDeclaration.Identifier.Text}";
                        return true;
                    }
                }
            }

            fullName = null;
            return false;
        }
    }
}