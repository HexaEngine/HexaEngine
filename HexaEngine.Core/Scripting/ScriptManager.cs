namespace HexaEngine.Core.Scripting
{
    using HexaEngine.Core.Debugging;
    using NLua;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class ScriptManager
    {
        private static readonly LuaFactory luaFactory = new();
        private static readonly List<IScript> scripts = new();

        public static IReadOnlyList<IScript> Scripts => scripts;

        public static void Reload()
        {
            scripts.Clear();
        }

        public static void RegisterFunc(string name, MethodBase method)
        {
            luaFactory.RegisterFunction(name, method);
        }
    }

    public class SomeClass
    {
        public string MyProperty { get; private set; }

        public SomeClass(string param1 = "defaulValue")
        {
            MyProperty = param1;
        }

        public int Func1()
        {
            return 32;
        }

        public string AnotherFunc(int val1, string val2)
        {
            return "Some String";
        }

        public static string StaticMethod(int param)
        {
            return "Return of Static Method";
        }
    }

    public class LuaFactory
    {
        public readonly Lua Lua = new();

        public LuaFactory()
        {
            Lua.RegisterFunction("print", typeof(LuaFactory).GetMethod(nameof(Print)));
            Lua.RegisterFunction("sleep", typeof(LuaFactory).GetMethod(nameof(Sleep)));
            Lua.LoadCLRPackage();
        }

        public LuaConsoleScript Create(string file)
        {
            return new LuaConsoleScript(this, file);
        }

        public void RegisterFunction(string name, MethodBase method)
        {
            Lua.RegisterFunction(name, method);
        }

        public static void Print(string text)
        {
            ImGuiConsole.LogAsync(text).Wait();
        }

        public static void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
    }

    public class LuaConsoleScript : IScript
    {
        private readonly LuaFunction function;

        public LuaConsoleScript(LuaFactory factory, string file)
        {
            function = factory.Lua.LoadString(File.ReadAllText(file), file);
            Name = file;
        }

        public string Name { get; }

        public void Run()
        {
            function.Call();
        }

        public void Run(params object[] args)
        {
        }

        public Task RunAsync()
        {
            return Task.Run(() =>
            {
                function.Call();
            });
        }

        public Task RunAsync(params object[] args)
        {
            throw new NotImplementedException();
        }

        public Task RunAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task RunAsync(CancellationToken token, params object[] args)
        {
            throw new NotImplementedException();
        }
    }

    public interface IScript
    {
        string Name { get; }

        public void Run();

        public void Run(params object[] args);

        public Task RunAsync();

        public Task RunAsync(CancellationToken token);

        public Task RunAsync(params object[] args);

        public Task RunAsync(CancellationToken token, params object[] args);
    }

    public class LuaScript : IScript
    {
        private readonly string code;
        private readonly LuaFactory luaFactory;

        public LuaScript(LuaFactory factory, string file)
        {
            luaFactory = factory;
            Name = file;
            code = File.ReadAllText(file);
        }

        public string Name { get; }

        public void Run()
        {
            luaFactory.Lua.DoString(code);
        }

        public void Run(params object[] args)
        {
        }

        public Task RunAsync()
        {
            return Task.Run(() =>
            {
                luaFactory.Lua.DoString(code);
            });
        }

        public Task RunAsync(params object[] args)
        {
            throw new NotImplementedException();
        }

        public Task RunAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task RunAsync(CancellationToken token, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}