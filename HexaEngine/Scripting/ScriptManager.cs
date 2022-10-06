namespace HexaEngine.Scripting
{
    using NLua;

    public static class ScriptManager
    {
        public static readonly Lua Lua = new();

        public static LuaFunction Load(string path)
        {
            return Lua.LoadFile(path);
        }
    }

    public class LuaScript
    {
        private readonly LuaFunction function;

        public LuaScript(LuaFunction function)
        {
            this.function = function;
        }

        public void Execute()
        {
            function.Call();
        }
    }
}