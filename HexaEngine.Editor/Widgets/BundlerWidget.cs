using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets.Dialogs;
using Hexa.NET.Logging;
using Hexa.NET.Utilities.IO;
using Hexa.NET.Utilities.Text;
using HexaEngine.Core.Assets;
using HexaEngine.Core.Graphics;
using HexaEngine.Core.IO.Binary.Archives;
using HexaEngine.Core.Logging;
using HexaEngine.Editor.Extensions;
using System.Numerics;

namespace HexaEngine.Editor.Widgets
{
    public class BundlerWidget : EditorWindow
    {
        protected override string Name { get; } = "Asset Bundler";

        public class AssetNamespaceItem
        {
            public string Name = string.Empty;
            public List<AssetEntry> Assets = [];
        }

        public struct AssetEntry
        {
            public string SourcePath;
            public string ArchivePath;
        }

        private readonly List<AssetNamespaceItem> namespaces = [];
        private Task? task;
        private float progress = 0;

        public bool IsBusy => task != null && !task.IsCompleted;

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            byte* buffer = stackalloc byte[1024];
            StrBuilder builder = new(buffer, 1024);

            if (IsBusy)
            {
                ImGui.ProgressBar(progress, new Vector2(-1, 0), "Creating Archive..."u8);
            }

            ImGui.BeginDisabled(IsBusy);

            for (int i = 0; i < namespaces.Count; i++)
            {
                AssetNamespaceItem ns = namespaces[i];
                ImGui.BeginGroup();

                ImGui.InputText("Namespace"u8, ref ns.Name, 1024);

                if (ImGui.BeginTable(builder.BuildId(i), 3, ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable))
                {
                    ImGui.TableSetupColumn("Source Path"u8, ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Archive Path"u8, ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn(""u8, 80.0f);
                    ImGui.TableHeadersRow();

                    for (int j = 0; j < ns.Assets.Count; j++)
                    {
                        AssetEntry entry = ns.Assets[j];
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.InputText(builder.BuildId(i, j * 3), ref entry.SourcePath, 1024);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.InputText(builder.BuildId(i, j * 3 + 1), ref entry.ArchivePath, 1024);
                        ImGui.TableSetColumnIndex(2);
                        if (ImGui.Button(builder.BuildLabelId(FontAwesome.Trash, (i << 16) + j * 3 + 2)))
                        {
                            ns.Assets.RemoveAt(j);
                            j--;
                        }
                    }

                    ImGui.EndTable();
                }

                if (ImGui.Button(builder.BuildLabelId(FontAwesome.FileCirclePlus, i)))
                {
                    OpenFileDialog dialog = new();
                    dialog.Userdata = ns;
                    dialog.AllowMultipleSelection = true;
                    dialog.Show(OnAddFile);
                }

                if (ImGui.Button(builder.BuildLabelId(FontAwesome.FolderPlus, i)))
                {
                    OpenFileDialog dialog = new();
                    dialog.OnlyAllowFolders = true;
                    dialog.Userdata = ns;
                    dialog.Show(OnAddFolder);
                }

                ImGui.EndGroup();
            }

            if (ImGui.Button("Add Namespace"u8))
            {
                namespaces.Add(new AssetNamespaceItem());
            }

            if (ImGui.Button("Create"u8))
            {
                SaveFileDialog dialog = new();
                dialog.Show(OnCreateArchive);
            }

            ImGui.EndDisabled();
        }

        private void OnAddFolder(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok) return;
            var dialog = (OpenFileDialog)sender!;
            var ns = (AssetNamespaceItem)dialog.Userdata!;
            var folder = dialog.SelectedFile!;
            foreach (var entry in FileUtils.EnumerateEntries(folder, "*.*", SearchOption.AllDirectories))
            {
                var srcPath = entry.Path.ToString();
                var relPath = Path.GetRelativePath(folder, srcPath);
                ns.Assets.Add(new AssetEntry
                {
                    SourcePath = srcPath,
                    ArchivePath = relPath
                });
            }
        }

        private void OnAddFile(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok) return;
            var dialog = (OpenFileDialog)sender!;
            var ns = (AssetNamespaceItem)dialog.Userdata!;

            foreach (var file in dialog.Selection)
            {
                ns.Assets.Add(new AssetEntry
                {
                    SourcePath = file,
                    ArchivePath = Path.GetFileName(file)
                });
            }
        }

        private void OnCreateArchive(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || IsBusy) return;

            var dialog = (SaveFileDialog)sender!;
            task = Task.Run(() => CreateArchive(dialog.SelectedFile));
        }

        private void CreateArchive(string path)
        {
            int totalAssets = namespaces.Sum(x => x.Assets.Count);
            int processedAssets = 0;
            try
            {
                using AssetArchive currentArchive = new(path, AssetArchiveMode.Create);
                foreach (var ns in namespaces)
                {
                    var archiveNs = currentArchive.AddNamespace(ns.Name);
                    foreach (var asset in ns.Assets)
                    {
                        using var stream = File.OpenRead(asset.SourcePath);
                        currentArchive.AddEntry(archiveNs, stream, asset.ArchivePath, Path.GetFileName(asset.SourcePath), AssetType.Unknown, Guid.Empty, Guid.Empty);
                        processedAssets++;
                        progress = (float)processedAssets / totalAssets;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerFactory.General.LogAndShowError("Failed to create archive", ex);
            }
        }
    }
}