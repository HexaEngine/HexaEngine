using System.Collections.Immutable;

namespace HexaEngine.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidExpensiveOperationsInUpdateAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "HEA001";
        private static readonly LocalizableString Title = "Avoid expensive operations in Update method";
        private static readonly LocalizableString MessageFormat = "Avoid calling '{0}' in the Update method";
        private static readonly LocalizableString Description = "Avoid using expensive operations like GetComponent, new objects, or Substring in the Update method.";
        private const string Category = "Performance";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static readonly string[] expensiveMethods =
        [
            "GetComponent",
            "GetDepth",
            "FindParent",
            "FindChild",
            "GetComponentFromParent",
            "GetComponentsFromParents",
            "GetComponentFromChild",
            "GetComponentsFromChilds",
            "GetOrCreateComponent",
            "TryGetComponent",
            "GetComponents",
            "HasComponent",
            "GetComponentsFromTree",
            "DiscoverComponents",
            "GetChild",
            "TryGetChild",
            "GetChildren"
        ];

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var memberAccessExpr = invocationExpr.Expression as SimpleNameSyntax;

            if (memberAccessExpr != null && expensiveMethods.Contains(memberAccessExpr.Identifier.ToString()))
            {
                var methodDecl = invocationExpr.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (methodDecl != null && methodDecl.Identifier.Text == "Update")
                {
                    var classDecl = methodDecl.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                    if (classDecl != null && InheritsFromScriptBehaviour(classDecl, context.SemanticModel))
                    {
                        var diagnostic = Diagnostic.Create(Rule, invocationExpr.GetLocation(), memberAccessExpr.Identifier.ToString());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool InheritsFromScriptBehaviour(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
        {
            var baseType = classDecl.BaseList?.Types.FirstOrDefault();
            if (baseType != null)
            {
                var typeSymbol = semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
                while (typeSymbol != null)
                {
                    if (typeSymbol.ToString() == "HexaEngine.Scripts.ScriptBehaviour")
                    {
                        return true;
                    }
                    typeSymbol = typeSymbol.BaseType;
                }
            }
            return false;
        }
    }
}