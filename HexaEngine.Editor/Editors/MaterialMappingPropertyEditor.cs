namespace HexaEngine.Editor.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Reflection;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Components.Renderer;

    public class MaterialMappingPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;

        public MaterialMappingPropertyEditor(string name, PropertyInfo property)
        {
            Name = name;
            Property = property;
            guiName = new(name);
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public unsafe bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            if (value is not MaterialMappingCollection mappings)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);

            bool result = false;

            var materials = MaterialManager.Current.Materials;

            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{mapping.Mesh.Name}");
                ImGui.TableSetColumnIndex(1);
                var material = mapping.Material;

                if (material == null)
                {
                    if (ImGui.BeginCombo($"{guiName.Id}{mapping.Mesh.Name}", (byte*)null))
                    {
                        for (int j = 0; j < materials.Count; j++)
                        {
                            var mat = materials[j];
                            if (ImGui.Selectable(mat.Name, false))
                            {
                                value = mat;
                                result = true;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
                else
                {
                    if (ImGui.BeginCombo(guiName.Id, material.Name))
                    {
                        for (int j = 0; j < materials.Count; j++)
                        {
                            var mat = materials[j];
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
                }

                ImGui.SameLine();

                if (ImGui.SmallButton($"\xE70F{guiName.Id}{mapping.Mesh.Name}"))
                {
                    if (WindowManager.TryGetWindow<MaterialEditorWindow>(out var materialEditor))
                    {
                        materialEditor.Material = material;
                        materialEditor.Focus();
                    }
                }
            }

            return result;
        }
    }
}