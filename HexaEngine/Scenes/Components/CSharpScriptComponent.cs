namespace HexaEngine.Scenes.Components
{
    using HexaEngine.Core;
    using HexaEngine.Core.Collections;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scripts;
    using HexaEngine.Editor.Properties;
    using ImGuiNET;

    [EditorComponent<CSharpScriptComponent>("C# Script")]
    public class CSharpScriptComponent : IScriptComponent
    {
        private ScriptFlags flags;
        private IScript? instance;

        static CSharpScriptComponent()
        {
            ObjectEditorFactory.RegisterEditor(typeof(CSharpScriptComponent), new CSharpScriptEditor());
        }

        public string? ScriptType { get; set; }

        [JsonIgnore]
        public ScriptFlags Flags => flags;

        public event Action<IHasFlags<ScriptFlags>, ScriptFlags>? FlagsChanged;

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            if (ScriptType == null)
            {
                return;
            }

            if (!Application.InDesignMode)
            {
                Type? type = AssemblyManager.GetType(ScriptType);
                if (type == null)
                {
                    ImGuiConsole.Log($"Couldn't load script: {ScriptType}");
                    return;
                }

                try
                {
                    var methods = type.GetMethods();
                    flags = ScriptFlags.None;
                    for (int i = 0; i < methods.Length; i++)
                    {
                        var method = methods[i];
                        switch (method.Name)
                        {
                            case "Awake":
                                flags |= ScriptFlags.Awake;
                                break;

                            case "Update":
                                flags |= ScriptFlags.Update;
                                break;

                            case "FixedUpdate":
                                flags |= ScriptFlags.FixedUpdate;
                                break;

                            case "Destroy":
                                flags |= ScriptFlags.Destroy;
                                break;

                            default:
                                continue;
                        }
                    }

                    FlagsChanged?.Invoke(this, flags);

                    instance = Activator.CreateInstance(type) as IScript;
                }
                catch (Exception e)
                {
                    ImGuiConsole.Log(e);
                }

                if (instance != null)
                {
                    instance.GameObject = gameObject;
                    try
                    {
                        instance.Awake();
                    }
                    catch (Exception e)
                    {
                        ImGuiConsole.Log(e);
                    }
                }
            }
        }

        public void Update()
        {
            if (Application.InDesignMode || instance == null)
            {
                return;
            }

            try
            {
                instance.Update();
            }
            catch (Exception e)
            {
                ImGuiConsole.Log(e);
            }
        }

        public void FixedUpdate()
        {
            if (Application.InDesignMode || instance == null)
            {
                return;
            }

            try
            {
                instance.FixedUpdate();
            }
            catch (Exception e)
            {
                ImGuiConsole.Log(e);
            }
        }

        public void Destory()
        {
            if (Application.InDesignMode || instance == null)
            {
                return;
            }

            try
            {
                instance.Destroy();
            }
            catch (Exception e)
            {
                ImGuiConsole.Log(e);
            }

            instance = null;
        }
    }

    public class CSharpScriptEditor : IObjectEditor
    {
        public string Name => "C# Script";

        public Type Type => typeof(CSharpScriptComponent);

        public object? Instance { get; set; }
        public bool IsEmpty => false;

        public void Draw()
        {
            if (Instance is not CSharpScriptComponent component)
            {
                return;
            }

            var types = AssemblyManager.GetAssignableTypes(typeof(IScript));
            var names = AssemblyManager.GetAssignableTypeNames(typeof(IScript));

            var type = component.ScriptType != null ? AssemblyManager.GetType(component.ScriptType) : null;

            int index = type != null ? types.IndexOf(type) : -1;

            if (ImGui.Combo("Script", ref index, names, names.Length))
            {
                component.ScriptType = types[index].FullName;
            }
        }
    }
}