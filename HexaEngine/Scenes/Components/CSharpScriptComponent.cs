namespace HexaEngine.Scenes.Components
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scripting;
    using ImGuiNET;
    using System.Diagnostics;

    [EditorComponent<CSharpScriptComponent>("C# Script")]
    public class CSharpScriptComponent : IComponent
    {
        private IScript? instance;

        static CSharpScriptComponent()
        {
            ObjectEditorFactory.RegisterEditor(typeof(CSharpScriptComponent), new CSharpScriptEditor());
        }

        public string? ScriptType { get; set; }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            if (ScriptType == null) return;
            if (!Application.InDesignMode)
            {
                Debug.WriteLine($"Loading script: {ScriptType}");
                Type? type = AssemblyManager.GetType(ScriptType);
                if (type == null)
                {
                    Debug.WriteLine($"Couldn't load script: {ScriptType}");
                    return;
                }

                try
                {
                    instance = Activator.CreateInstance(type) as IScript;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
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
                        Debug.WriteLine(e);
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
                Debug.WriteLine(e);
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
                Debug.WriteLine(e);
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
                Debug.WriteLine(e);
            }
        }
    }

    public class CSharpScriptEditor : IObjectEditor
    {
        public string Name => "C# Script";

        public Type Type => typeof(CSharpScriptComponent);

        public object? Instance { get; set; }

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