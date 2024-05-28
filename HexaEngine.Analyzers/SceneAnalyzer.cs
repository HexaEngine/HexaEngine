namespace HexaEngine.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SceneAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "HEA004";
        private static readonly LocalizableString Title = "Scene methods should be called inside Scene.Dispatcher.Invoke";
        private static readonly LocalizableString MessageFormat = "Call to '{0}' should be wrapped inside 'Scene.Dispatcher.Invoke'";
        private static readonly LocalizableString Description = "Ensure Scene method calls are wrapped inside Scene.Dispatcher.Invoke for proper execution.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private static readonly string[] MethodsToCheck = ["AddChild", "AddChildUnsafe", "RemoveChild"];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;

            if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
            {
                return;
            }

            var methodName = memberAccessExpr.Name.Identifier.Text;
            var containingType = context.SemanticModel.GetTypeInfo(memberAccessExpr.Expression).Type?.ToDisplayString();

            if (containingType == "HexaEngine.Scenes.Scene" && MethodsToCheck.Contains(methodName))
            {
                var parentMethod = invocationExpr.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (parentMethod != null && !IsInsideDispatcherInvoke(invocationExpr, context.SemanticModel))
                {
                    var diagnostic = Diagnostic.Create(Rule, memberAccessExpr.GetLocation(), methodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsInsideDispatcherInvoke(InvocationExpressionSyntax invocationExpr, SemanticModel model)
        {
            var invocationAncestors = invocationExpr.Ancestors().OfType<InvocationExpressionSyntax>();

            foreach (var ancestor in invocationAncestors)
            {
                if (ancestor.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "Invoke")
                {
                    var containingType = model.GetTypeInfo(memberAccess.Expression).Type?.ToDisplayString();
                    if (containingType == "HexaEngine.Scenes.SceneDispatcher")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}