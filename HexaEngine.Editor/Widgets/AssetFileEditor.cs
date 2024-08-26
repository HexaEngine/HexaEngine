namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Properties;
    using System;
    using System.Collections.Generic;

    public class AssetFileEditor : IPropertyObjectEditor<AssetFileInfo>
    {
        public bool CanEditMultiple { get; } = false;

        public void Edit(IGraphicsContext context, AssetFileInfo assetFile)
        {
            ImGui.Text($"{assetFile.Name} File Properties");

            if (ImGui.CollapsingHeader("Misc", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGui.BeginTable("FileMeta", 2, ImGuiTableFlags.SizingFixedSame);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.None);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("File Name");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(assetFile.Name);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Full Path");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(assetFile.Path);

                ImGui.EndTable();
            }

            if (assetFile.Metadata == null)
            {
                return;
            }

            SourceAssetMetadata metadata = assetFile.Metadata;

            if (!AssetImporterRegistry.TryGetImporterForFile(Path.GetExtension(assetFile.Path.AsSpan()), out IAssetImporter? assetImporter))
            {
                return;
            }

            if (assetImporter.SettingsType == null || assetImporter.SettingsKey == null || assetImporter.SettingsDisplayName == null)
            {
                return;
            }

            var editor = ObjectEditorFactory.CreateEditor(assetImporter.SettingsType);

            if (!metadata.Additional.TryGetValue(assetImporter.SettingsKey, out var value))
            {
                return;
            }

            if (ImGui.CollapsingHeader(assetImporter.SettingsDisplayName, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                editor.NoTable = false;
                editor.Instance = value;
                bool changed = editor.Draw(context);

                if (changed)
                {
                    SourceAssetsDatabase.UpdateAsync(metadata, true);
                }
            }
        }

        public void EditMultiple(IGraphicsContext context, ICollection<object> objects)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}