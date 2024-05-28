namespace HexaEngine.Analyzers.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Testing.NUnit;
    using Microsoft.CodeAnalysis.Testing;

    public class IDisposableAnalyzerTests
    {
        [Test]
        public async Task TestIDisposableNotDisposed()
        {
            var testCode = AvoidExpensiveOperationsInUpdateAnalyzerTests.MockScriptBehaviour + @"

public class TestScript : HexaEngine.Scripts.ScriptBehaviour
{
    private SomeClass obj = new();
    void Awake()
    {
    }

    void Update()
    {
        var component = GetComponent<SomeComponent>();
    }
}";
            var expected = new DiagnosticResult(IDisposableAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithSpan(84, 23, 84, 34)
                .WithMessage("IDisposable object 'obj' is not disposed");

            await AnalyzerVerifier<IDisposableAnalyzer>.VerifyAnalyzerAsync(testCode, expected);
        }

        [Test]
        public async Task TestIDisposableNotDisposedNoWarn()
        {
            var testCode = AvoidExpensiveOperationsInUpdateAnalyzerTests.MockScriptBehaviour + @"

public class TestScript : HexaEngine.Scripts.ScriptBehaviour
{
    private SomeClass obj = new();
    void Awake()
    {
    }

    void Update()
    {
        var component = GetComponent<SomeComponent>();
    }

    void Destroy()
    {
        obj.Dispose();
    }
}";

            await AnalyzerVerifier<IDisposableAnalyzer>.VerifyAnalyzerAsync(testCode);
        }
    }
}