namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.Properties;
    using System.Reflection;

    public class AssetRefPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly AssetType assetType;

        public AssetRefPropertyEditor(string name, PropertyInfo property, AssetType assetType)
        {
            Name = name;
            Property = property;
            this.assetType = assetType;
            guiName = new(name);
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public unsafe bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            AssetRef val = (AssetRef)value!;

            bool changed = ComboHelper.ComboForAssetRef(guiName.Id, ref val, assetType);
            if (changed)
            {
                value = val;
            }

            if (assetType == AssetType.Material)
            {
                ImGui.SameLine();

                if (ImGui.SmallButton($"\xf304{guiName.Id}"))
                {
                    if (WindowManager.TryGetWindow<MaterialEditorWindow>(out var materialEditor))
                    {
                        materialEditor.Material = val;
                        materialEditor.Focus();
                    }
                }
            }

            return changed;
        }
    }
}