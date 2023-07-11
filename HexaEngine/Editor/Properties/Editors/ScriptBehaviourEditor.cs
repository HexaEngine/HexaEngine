namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Components;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scripts;
    using ImGuiNET;

    public class ScriptBehaviourEditor : IObjectEditor
    {
        private ImGuiName guiName = new("Script Behaviour");
        private IObjectEditor? editor;

        public string Name => "Script Behaviour";

        public Type Type => typeof(ScriptBehaviour);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool NoTable { get; set; }

        public void Dispose()
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (Instance is not ScriptBehaviour component)
            {
                return;
            }

            var types = AssemblyManager.GetAssignableTypes(typeof(IScriptBehaviour));
            var names = AssemblyManager.GetAssignableTypeNames(typeof(IScriptBehaviour));

            var type = component.ScriptType != null ? AssemblyManager.GetType(component.ScriptType) : null;

            int index = type != null ? types.IndexOf(type) : -1;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            if (ImGui.Combo(guiName.Id, ref index, names, names.Length))
            {
                if (editor != null)
                    editor.Instance = null;
                component.ScriptType = types[index].FullName;
                editor = null;
                type = types[index];
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

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            if (editor != null && editor.Instance != null && !editor.IsEmpty)
            {
                ImGui.Separator();
                editor.Draw(context);
            }
        }
    }
}