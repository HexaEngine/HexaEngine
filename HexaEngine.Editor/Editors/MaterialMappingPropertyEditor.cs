namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

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
            if (value is not MaterialAssetMapper mappings)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);

            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{mapping.Mesh}");
                ImGui.TableSetColumnIndex(1);
                var val = mapping.Material;

                bool changed = ComboHelper.ComboForAssetRef($"{guiName.Id}{mapping.Mesh}", ref val, AssetType.Material);
                if (changed)
                {
                    mapping.Material = val;
                    mappings[i] = mapping;
                }

                ImGui.SameLine();

                if (ImGui.SmallButton($"\xf304{guiName.Id}{mapping.Mesh}"))
                {
                    if (WindowManager.TryGetWindow<MaterialEditorWindow>(out var materialEditor))
                    {
                        materialEditor.Material = val;
                        materialEditor.Focus();
                    }
                }
            }

            return false;
        }
    }
}