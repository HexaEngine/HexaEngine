﻿namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using Hexa.NET.ImGui;
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