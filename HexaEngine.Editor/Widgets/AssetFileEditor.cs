namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Properties;
    using System;
    using System.Collections.Generic;

    public class AssetFileEditor : IPropertyObjectEditor<AssetFileInfo>
    {
        public bool CanEditMultiple { get; } = false;

        public unsafe void Edit(IGraphicsContext context, AssetFileInfo assetFile)
        {
            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);

            builder.Reset();
            builder.Append(assetFile.Name);
            builder.Append(" File Properties"u8);
            builder.End();
            ImGui.Text(builder);

            if (ImGui.CollapsingHeader("Misc"u8, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGui.BeginTable("FileMeta"u8, 2, ImGuiTableFlags.SizingFixedSame);
                ImGui.TableSetupColumn(""u8, ImGuiTableColumnFlags.None);
                ImGui.TableSetupColumn(""u8, ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("File Name"u8);
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(assetFile.Name);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Full Path"u8);
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