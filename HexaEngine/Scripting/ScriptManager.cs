namespace HexaEngine.Scripting
{
    using NLua;

    public static class ScriptManager
    {
    }

    public class LuaScript
    {
        private readonly Lua context = new();
        private LuaFunction? UpdateFunc;
        private LuaFunction? FixedUpdateFunc;
        private LuaFunction? AwakeFunc;
        private LuaFunction? DestroyFunc;

        public Lua Context => context;
    }
}