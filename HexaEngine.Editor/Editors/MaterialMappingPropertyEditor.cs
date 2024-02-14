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
    using HexaEngine.Core.Assets;

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
            if (value is not MaterialAssetMappingCollection mappings)
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

                var meta = val.GetMetadata();

                bool isOpen;
                if (meta != null)
                {
                    isOpen = ImGui.BeginCombo($"{guiName.Id}{mapping.Mesh}", meta.Name);
                }
                else
                {
                    if (val.Guid == Guid.Empty)
                    {
                        isOpen = ImGui.BeginCombo($"{guiName.Id}{mapping.Mesh}", (byte*)null);
                    }
                    else
                    {
                        isOpen = ImGui.BeginCombo($"{guiName.Id}{mapping.Mesh}", $"{val.Guid}");
                    }
                }

                if (isOpen)
                {
                    foreach (var asset in ArtifactDatabase.GetArtifactsFromType(AssetType.Material))
                    {
                        var mat = asset;
                        bool isSelected = val.Guid == asset.Guid;
                        if (ImGui.Selectable(mat.Name, isSelected))
                        {
                            val.Guid = asset.Guid;
                            mapping.Material = val;
                            mappings[i] = mapping;
                        }
                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.SameLine();

                if (ImGui.SmallButton($"\xE70F{guiName.Id}{mapping.Mesh}"))
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