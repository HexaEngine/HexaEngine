namespace HexaEngine.Scenes.Components
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using NLua;

    [EditorComponent<LuaScriptComponent>("Lua Script")]
    public class LuaScriptComponent : IComponent
    {
        private GameObject node;
        private readonly Lua context;
        private LuaFunction? UpdateFunc;
        private LuaFunction? FixedUpdateFunc;
        private LuaFunction? AwakeFunc;
        private LuaFunction? DestroyFunc;
        private string? file = string.Empty;
        private bool update = true;
        private bool error = false;

        public LuaScriptComponent()
        {
            context = new Lua();
            context.RegisterFunction("print", typeof(LuaScriptComponent).GetMethod(nameof(Print)));
        }

        public static void Print(string text)
        {
            ImGuiConsole.LogAsync(text).Wait();
        }

        [EditorProperty("Script")]
        public string? File
        { get => file; set { file = value; update = true; } }

        private void Load()
        {
            if (file != null && FileSystem.Exists(Paths.CurrentScriptFolder + file) && update)
            {
                try
                {
                    update = false;

                    context.DoString(FileSystem.ReadAllText(Paths.CurrentScriptFolder + file), Paths.CurrentScriptFolder + file);
                    context["component"] = this;
                    context["node"] = node;
                    context["scene"] = node.GetScene();
                    UpdateFunc = context["UpdateAssemblies"] as LuaFunction;
                    FixedUpdateFunc = context["FixedUpdate"] as LuaFunction;
                    AwakeFunc = context["Awake"] as LuaFunction;
                    DestroyFunc = context["Destroy"] as LuaFunction;
                    if (Application.InDesignMode) return;
                    AwakeFunc?.Call();
                }
                catch (Exception e)
                {
                    ImGuiConsole.Log(e);
                }
            }
        }

        public void Awake(IGraphicsDevice device, GameObject node)
        {
            this.node = node;
            Load();
        }

        public void FixedUpdate()
        {
            Load();
            FixedUpdateFunc?.Call();
        }

        public void Update()
        {
            Load();
            context["TimeDelta"] = Time.Delta;
            UpdateFunc?.Call();
        }

        public void Destory()
        {
            Load();
            DestroyFunc?.Call();
        }
    }
}