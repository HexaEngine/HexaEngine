namespace HexaEngine.Core.Editor.Properties.Editors
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.UI;
    using ImGuiNET;
    using System.Reflection;

    public class ObjectEditorButton
    {
        private ImGuiName guiName;
        private readonly MethodInfo info;

        public ObjectEditorButton(EditorButtonAttribute attribute, MethodInfo info)
        {
            guiName = new(attribute.Name);
            this.info = info;
        }

        public void Draw(object instance)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);
            if (ImGui.Button(guiName.UniqueName))
            {
                info.Invoke(instance, null);
            }
        }
    }
}