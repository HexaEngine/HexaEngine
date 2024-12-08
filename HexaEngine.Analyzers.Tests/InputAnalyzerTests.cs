namespace HexaEngine.Analyzers.Tests
{
    using HexaEngine.Analyzers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Testing;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.NUnit.AnalyzerVerifier<InputAnalyzer>;

    public class InputAnalyzerTests
    {
        public const string MockInput = @"
namespace HexaEngine.Input
{
    public static class Input
    {
        public static float GetAxis(string name)
        {
            return 0;
        }
    }
}
";

        [Test]
        public async Task TestDuplicateGetAxisCalls()
        {
            var testCode = AvoidExpensiveOperationsInUpdateAnalyzerTests.MockScriptBehaviour + MockInput + @"
namespace Test
{
    using HexaEngine.Input;

    public class TestScript : HexaEngine.Scripts.ScriptBehaviour
    {
        public void Update()
        {
            float axis1 = Input.GetAxis(""Horizontal"");
            float axis2 = Input.GetAxis(""Horizontal"");
        }
    }
}";

            var expected1 = new DiagnosticResult(InputAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithSpan(100, 27, 100, 54)
                .WithMessage("Input.GetAxis(\"Horizontal\") is called multiple times in Update with the same name");
            var expected2 = new DiagnosticResult(InputAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithSpan(101, 27, 101, 54)
                .WithMessage("Input.GetAxis(\"Horizontal\") is called multiple times in Update with the same name");

            await VerifyCS.VerifyAnalyzerAsync(testCode, expected1, expected2);
        }

        [Test]
        public async Task TestSingleGetAxisCall()
        {
            var testCode = AvoidExpensiveOperationsInUpdateAnalyzerTests.MockScriptBehaviour + MockInput + @"
namespace Test
{
    using HexaEngine.Input;

    public class TestScript : HexaEngine.Scripts.ScriptBehaviour
    {
        public void Update()
        {
            float axis1 = Input.GetAxis(""Horizontal"");
            float axis2 = Input.GetAxis(""Vertical"");
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}