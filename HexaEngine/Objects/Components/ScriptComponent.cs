namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using HexaEngine.Scripting;
    using NLua;
    using System.Numerics;

    [EditorComponent<ScriptComponent>("Script")]
    public class ScriptComponent : IComponent, IScript
    {
        private SceneNode node;
        private readonly Lua context;
        private LuaFunction? UpdateFunc;
        private LuaFunction? FixedUpdateFunc;
        private LuaFunction? AwakeFunc;
        private LuaFunction? DestroyFunc;
        private string? file = string.Empty;
        private bool update = true;

        public ScriptComponent()
        {
            context = new Lua();
            context.RegisterFunction("print", typeof(ScriptComponent).GetMethod(nameof(Print)));
            Editor = new PropertyEditor<ScriptComponent>(this);
        }

        public static void Print(string text)
        {
            ImGuiConsole.LogAsync(text).Wait();
        }

        [EditorProperty("Script")]
        public string? File
        { get => file; set { file = value; update = true; } }

        public IPropertyEditor? Editor { get; }

        private void Load()
        {
            if (file != null && FileSystem.Exists(Paths.CurrentScriptFolder + file) && update)
            {
                update = false;

                context.DoString(FileSystem.ReadAllText(Paths.CurrentScriptFolder + file), Paths.CurrentScriptFolder + file);
                context["component"] = this;
                context["node"] = node;
                context["scene"] = node.GetScene();
                UpdateFunc = context["Update"] as LuaFunction;
                FixedUpdateFunc = context["FixedUpdate"] as LuaFunction;
                AwakeFunc = context["Awake"] as LuaFunction;
                DestroyFunc = context["Destroy"] as LuaFunction;
                if (Designer.InDesignMode) return;
                AwakeFunc?.Call();
            }
        }

        public void Initialize(IGraphicsDevice device, SceneNode node)
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

        public void Uninitialize()
        {
            Load();
            DestroyFunc?.Call();
        }
    }
}