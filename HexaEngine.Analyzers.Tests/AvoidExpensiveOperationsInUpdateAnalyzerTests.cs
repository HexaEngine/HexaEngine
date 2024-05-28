using Microsoft;

namespace HexaEngine.Analyzers.Tests
{
    using Microsoft;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Testing.NUnit;
    using Microsoft.CodeAnalysis.Testing;
    using NUnit.Framework;

    public class AvoidExpensiveOperationsInUpdateAnalyzerTests
    {
        public const string MockScriptBehaviour = @"
namespace HexaEngine.Scripts
{
    public class ScriptBehaviour
    {
        public T GetComponent<T>() => default;
        public HexaEngine.Scenes.GameObject GameObject;
        public HexaEngine.Scenes.Scene Scene;
    }

    public class GameObject
    {
        public T GetComponentFromParent<T>() => default;
    }
}

public class SomeComponent : HexaEngine.Scenes.IComponent
{
    public System.Guid Guid { get; set; }

    public HexaEngine.Scenes.GameObject GameObject { get; set; }

    public bool IsSerializable { get; }

    public void Awake()
    {
    }

    public void Destroy()
    {
    }
}

public class SomeClass : System.IDisposable
{
    public void Dispose()
    {
    }
}

namespace HexaEngine.Scenes
{
    public class GameObject
    {
    }

    public class SceneDispatcher
    {
        public void Invoke(object? context, System.Action<object?> action)
        {
        }

        public void Invoke<T>(T context, System.Action<T> action)
        {
        }
    }

    public class Scene
    {
       public SceneDispatcher Dispatcher { get; } = new();

       public void AddChildUnsafe(GameObject child)
       {
       }
    }

    public interface IComponent
    {
        System.Guid Guid { get; set; }

        GameObject GameObject { get; set; }

        bool IsSerializable { get; }

        void Awake();

        void Destroy();
    }
}
";

        [Test]
        public async Task DetectsGetComponentInUpdateMethod()
        {
            var testCode = MockScriptBehaviour + @"

public class TestScript : HexaEngine.Scripts.ScriptBehaviour
{
    void Update()
    {
        var component = GetComponent<SomeComponent>();
    }
}";
            var expected = new DiagnosticResult(AvoidExpensiveOperationsInUpdateAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithSpan(86, 25, 86, 54)
                .WithMessage("Avoid calling 'GetComponent' in the Update method");

            await AnalyzerVerifier<AvoidExpensiveOperationsInUpdateAnalyzer>.VerifyAnalyzerAsync(testCode, expected);
        }

        [Test]
        public async Task DetectsGetComponentNotInAwakeMethod()
        {
            var testCode = MockScriptBehaviour + @"

public class TestScript : HexaEngine.Scripts.ScriptBehaviour
{
    void Awake()
    {
        var component = GetComponent<SomeComponent>();
    }
}";

            await AnalyzerVerifier<AvoidExpensiveOperationsInUpdateAnalyzer>.VerifyAnalyzerAsync(testCode);
        }
    }
}