using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace HexaEngine.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InputAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "HEA005";
        private static readonly LocalizableString Title = "Duplicate Input.GetAxis call";
        private static readonly LocalizableString MessageFormat = "Input.GetAxis(\"{0}\") is called multiple times in Update with the same name";
        private static readonly LocalizableString Description = "Avoid calling Input.GetAxis with the same name multiple times in the Update method.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.Identifier.Text != "Update")
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            Dictionary<string, List<InvocationExpressionSyntax>> invocationNames = [];

            foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Name.Identifier.Text != "GetAxis")
                {
                    continue;
                }

                if (semanticModel.GetSymbolInfo(memberAccess).Symbol is not IMethodSymbol methodSymbol || methodSymbol.ContainingType.ToDisplayString() != "HexaEngine.Input.Input")
                {
                    continue;
                }

                var argumentList = invocation.ArgumentList.Arguments;
                if (argumentList.Count != 1)
                {
                    continue;
                }

                if (argumentList[0].Expression is not LiteralExpressionSyntax argument || argument.Kind() != SyntaxKind.StringLiteralExpression)
                {
                    continue;
                }

                var axisName = argument.Token.ValueText;
                if (!invocationNames.ContainsKey(axisName))
                {
                    invocationNames[axisName] = [];
                }

                invocationNames[axisName].Add(invocation);
            }

            foreach (var kvp in invocationNames)
            {
                if (kvp.Value.Count > 1)
                {
                    foreach (var invocation in kvp.Value)
                    {
                        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), kvp.Key);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}