namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scripts;

    public class ScriptBehaviourEditor : IObjectEditor
    {
        private ImGuiName guiName = new("Script Behaviour");
        private IObjectEditor? editor;

        public string Name => guiName.Name;

        public Type Type => typeof(ScriptBehaviour);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        public string Symbol { get; } = "\xf000";

        public void Dispose()
        {
        }

        public bool Draw(IGraphicsContext context)
        {
            if (Instance is not ScriptBehaviour component)
            {
                return false;
            }

            var types = AssemblyManager.GetAssignableTypes(typeof(IScriptBehaviour));
            var names = AssemblyManager.GetAssignableTypeNames(typeof(IScriptBehaviour));

            if (types.Count == 0)
            {
                return false;
            }

            bool changed = false;

            var type = component.ScriptType != null ? AssemblyManager.GetType(component.ScriptType) : null;

            int index = type != null ? types.IndexOf(type) : -1;

            if (ImGui.Combo(guiName.Id, ref index, names, names.Length))
            {
                if (editor != null)
                    editor.Instance = null;
                component.ScriptType = types[index].FullName;
                editor = null;
                type = types[index];
                changed = true;
            }

            if (editor == null && type != null)
            {
                editor = ObjectEditorFactory.CreateEditor(type);
                editor.Instance = component.Instance;
            }

            if (editor != null && editor.Instance == null)
            {
                editor.Instance = component.Instance;
            }

            if (editor != null && editor.Instance != null && !editor.IsEmpty)
            {
                ImGui.Separator();
                changed |= editor.Draw(context);
            }

            return changed;
        }
    }
}