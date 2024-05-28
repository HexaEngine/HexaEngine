namespace HexaEngine.Analyzers.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Testing;
    using NUnit.Framework;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.NUnit.AnalyzerVerifier<SceneAnalyzer>;

    public class SceneAnalyzerTests
    {
        [Test]
        public async Task TestAddChildUnsafeNotWrapped()
        {
            var testCode = AvoidExpensiveOperationsInUpdateAnalyzerTests.MockScriptBehaviour + @"
public class TestScript : HexaEngine.Scripts.ScriptBehaviour
{
    public void Awake()
    {
        Scene.AddChildUnsafe(new HexaEngine.Scenes.GameObject());
    }
}";

            var expected = new DiagnosticResult(SceneAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithSpan(85, 9, 85, 29)
                .WithMessage("Call to 'AddChildUnsafe' should be wrapped inside 'Scene.Dispatcher.Invoke'");

            await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
        }

        [Test]
        public async Task TestAddChildUnsafeWrapped()
        {
            var testCode = AvoidExpensiveOperationsInUpdateAnalyzerTests.MockScriptBehaviour + @"
public class TestScript : HexaEngine.Scripts.ScriptBehaviour
{
    public void Awake()
    {
        Scene.Dispatcher.Invoke(null, ctx =>
        {
            Scene.AddChildUnsafe(new HexaEngine.Scenes.GameObject());
        });
    }
}"
            ;

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}