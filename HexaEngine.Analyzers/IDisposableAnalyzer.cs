namespace HexaEngine.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IDisposableAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "HEA002";
        private static readonly LocalizableString Title = "IDisposable object is not disposed";
        private static readonly LocalizableString MessageFormat = "IDisposable object '{0}' is not disposed";
        private static readonly LocalizableString Description = "All IDisposable objects should be disposed of in the Destroy method or earlier.";
        private const string Category = "Usage";

        public const string CrucialDisposeDiagnosticId = "HEA003";
        private static readonly LocalizableString CrucialDisposeTitle = "Crucial IDisposable object is not disposed";
        private static readonly LocalizableString CrucialDisposeMessageFormat = "Crucial IDisposable object '{0}' is not disposed";
        private static readonly LocalizableString CrucialDisposeDescription = "IDisposable objects marked with [DisposeIsCrucial] must be disposed of in the Destroy method or earlier.";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor CrucialDisposeRule = new(CrucialDisposeDiagnosticId, CrucialDisposeTitle, CrucialDisposeMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: CrucialDisposeDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, CrucialDisposeRule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.FieldDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                if (context.SemanticModel.GetDeclaredSymbol(variable) is not IFieldSymbol fieldSymbol) continue;

                if (fieldSymbol.Type.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable"))
                {
                    bool isCrucial = false;
                    foreach (var attribute in fieldSymbol.Type.GetAttributes())
                    {
                        if (attribute.AttributeClass == null)
                            continue;
                        string name = attribute.AttributeClass.ToDisplayString();
                        isCrucial = name == "HexaEngine.Analyzers.Annotations.DisposeIsCrucialAttribute";
                        if (name == "HexaEngine.Analyzers.Annotations.DisposeIgnoreAttribute")
                        {
                            return;
                        }
                    }

                    var containingType = fieldSymbol.ContainingType;
                    var disposeMethod = containingType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == "Destroy");

                    if (disposeMethod == null || !IsDisposedInMethod(context, disposeMethod, fieldSymbol))
                    {
                        var rule = isCrucial ? CrucialDisposeRule : Rule;
                        var diagnostic = Diagnostic.Create(rule, variable.GetLocation(), fieldSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsDisposedInMethod(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, IFieldSymbol fieldSymbol)
        {
            // Check if the IDisposable field is disposed within the method.
            // This is a simplified check. You might want to enhance this to cover more cases.
            foreach (var syntaxReference in methodSymbol.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodSyntax) continue;

                var methodBody = methodSyntax.Body;
                if (methodBody == null) continue;

                var disposeStatements = methodBody.DescendantNodes()
                    .OfType<ExpressionStatementSyntax>()
                    .Where(s => s.Expression is InvocationExpressionSyntax invocation &&
                                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                memberAccess.Name.Identifier.Text == "Dispose");

                foreach (var disposeStatement in disposeStatements)
                {
                    var memberAccess = (MemberAccessExpressionSyntax)((InvocationExpressionSyntax)disposeStatement.Expression).Expression;
                    var disposedSymbol = context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol as IFieldSymbol;
                    if (disposedSymbol?.Name == fieldSymbol.Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}