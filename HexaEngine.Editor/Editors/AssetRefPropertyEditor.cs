namespace HexaEngine.Editor.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Reflection;
    using HexaEngine.Core.Assets;

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
            AssetRef val = (AssetRef)value;

            var meta = val.GetMetadata();

            bool isOpen;
            if (meta != null)
            {
                isOpen = ImGui.BeginCombo(guiName.Id, meta.Name);
            }
            else
            {
                if (val.Guid == Guid.Empty)
                {
                    isOpen = ImGui.BeginCombo(guiName.Id, (byte*)null);
                }
                else
                {
                    isOpen = ImGui.BeginCombo(guiName.Id, $"{val.Guid}");
                }
            }

            TooltipHelper.Tooltip($"{meta?.Name}#{val.Guid}");

            bool changed = false;
            if (isOpen)
            {
                foreach (var asset in ArtifactDatabase.GetArtifactsFromType(assetType))
                {
                    bool isSelected = val.Guid == asset.Guid;
                    if (ImGui.Selectable(asset.Name, isSelected))
                    {
                        val.Guid = asset.Guid;
                        value = val;
                        changed = true;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            return changed;
        }
    }
}