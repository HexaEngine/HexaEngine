namespace HexaEngine.Editor.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Reflection;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Editor.MaterialEditor;

    public class MaterialPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;

        public MaterialPropertyEditor(string name, PropertyInfo property)
        {
            Name = name;
            Property = property;
            guiName = new(name);
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            if (value is not MaterialData material)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);

            bool result = false;

            var materials = MaterialManager.Current.Materials;

            if (ImGui.BeginCombo(guiName.Id, material.Name))
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    var mat = materials[i];
                    var isSelected = mat == material;
                    if (ImGui.Selectable(mat.Name, isSelected))
                    {
                        value = mat;
                        result = true;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();

            if (ImGui.SmallButton($"\xE70F##{guiName.Id}"))
            {
                if (WindowManager.TryGetWindow<MaterialEditorWindow>(out var materialEditor))
                {
                    materialEditor.Material = material;
                    materialEditor.Focus();
                }
            }

            return result;
        }
    }
}