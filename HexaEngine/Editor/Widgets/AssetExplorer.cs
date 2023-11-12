#if ENABLE_OBSOLETE_CODE
namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Projects;
    using HexaEngine.Scenes.Serialization;
    using Hexa.NET.ImGui;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// The old asset explorer
    /// </summary>
    [Obsolete($"Use {nameof(AssetExplorer2)}")]
    public class AssetExplorer : EditorWindow
    {
        private DirectoryInfo? currentDir;
        private DirectoryInfo? parentDir;
        private readonly RenameFileDialog dialog = new(true);
        private readonly List<Item> files = new();
        private readonly List<Item> dirs = new();
        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();
        private string? CurrentFolder = null;

#pragma warning disable CS0169 // The field 'AssetExplorer.renameItem' is never used
        private Item? renameItem;
#pragma warning restore CS0169 // The field 'AssetExplorer.renameItem' is never used

        private struct Item
        {
            public string Path;
            public string Name;

            public Item(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }

        public AssetExplorer()
        {
            IsShown = false;
            Refresh();
            currentDir = null;
            parentDir = currentDir?.Parent;
#pragma warning disable CS8622 // Nullability of reference types in type of parameter 'obj' of 'void AssetExplorer.ProjectManager_ProjectLoaded(HexaProject obj)' doesn't match the target delegate 'Action<HexaProject?>' (possibly because of nullability attributes).
            ProjectManager.ProjectChanged += ProjectManager_ProjectLoaded;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter 'obj' of 'void AssetExplorer.ProjectManager_ProjectLoaded(HexaProject obj)' doesn't match the target delegate 'Action<HexaProject?>' (possibly because of nullability attributes).
        }

        private void ProjectManager_ProjectLoaded(HexaProject obj)
        {
            SetFolder(ProjectManager.CurrentProjectAssetsFolder);
        }

        public string? SelectedFile { get; private set; }

        protected override string Name => "Assets (Old)";

        public void Refresh()
        {
            FileSystem.Refresh();
            files.Clear();
            dirs.Clear();
            if (CurrentFolder == null)
            {
                return;
            }

            currentDir = new(CurrentFolder);
            parentDir = currentDir?.Parent;

            foreach (var fse in Directory.GetFileSystemEntries(CurrentFolder, string.Empty))
            {
                if (File.GetAttributes(fse).HasFlag(FileAttributes.System))
                {
                    continue;
                }

                if (File.GetAttributes(fse).HasFlag(FileAttributes.Hidden))
                {
                    continue;
                }

                if (File.GetAttributes(fse).HasFlag(FileAttributes.Device))
                {
                    continue;
                }

                if (Directory.Exists(fse))
                {
                    dirs.Add(new("\xe8b7" + Path.GetFileName(fse), fse));
                }
                else
                {
                    files.Add(new("\xe8a5" + Path.GetFileName(fse), fse));
                }
            }
        }

        public void SetFolder(string? path)
        {
            if (CurrentFolder != null)
            {
                backHistory.Push(CurrentFolder);
            }

            CurrentFolder = path;
            forwardHistory.Clear();
            Refresh();
        }

        public void GoHome()
        {
            CurrentFolder = Paths.CurrentProjectFolder;
            Refresh();
        }

        public void TryGoBack()
        {
            if (backHistory.TryPop(out var historyItem))
            {
                if (CurrentFolder != null)
                {
                    forwardHistory.Push(CurrentFolder);
                }

                CurrentFolder = historyItem;
                Refresh();
            }
        }

        public void TryGoForward()
        {
            if (forwardHistory.TryPop(out var historyItem))
            {
                if (CurrentFolder != null)
                {
                    backHistory.Push(CurrentFolder);
                }

                CurrentFolder = historyItem;
                Refresh();
            }
        }

        private void DisplayDir(Item dir)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
            if (ImGui.Selectable(dir.Name, false, ImGuiSelectableFlags.DontClosePopups))
            {
                SetFolder(dir.Path);
            }

            ImGui.PopStyleColor();

            ImGui.PushID(dir.Path);
            if (ImGui.BeginPopupContextItem(dir.Path))
            {
                if (ImGui.MenuItem("Open"))
                {
                    SetFolder(dir.Path);
                }
                if (ImGui.MenuItem("Delete"))
                {
                    Directory.Delete(dir.Path, true);
                }

                ImGui.EndPopup();
            }
            ImGui.PopID();

            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload(nameof(String));
                    if (!payload.IsNull)
                    {
                        string ft = *(UnsafeOldString*)payload.Data;
                        if (Directory.Exists(ft))
                        {
                            var fname = Path.GetFileName(ft);
                            Directory.Move(ft, Path.Combine(dir.Path, fname));
                        }
                        else if (File.Exists(ft))
                        {
                            File.Move(ft, dir.Path);
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }

            ImGuiDragDropFlags src_flags = 0;
            src_flags |= ImGuiDragDropFlags.SourceNoDisableHover;     // Keep the source displayed as hovered
            src_flags |= ImGuiDragDropFlags.SourceNoHoldToOpenOthers; // Because our dragging is local, we disable the feature of opening foreign treenodes/tabs while dragging
                                                                      //src_flags |= ImGuiDragDropFlags_SourceNoPreviewTooltip; // Hide the tooltip
            if (ImGui.BeginDragDropSource(src_flags))
            {
                unsafe
                {
                    var str = new UnsafeOldString(dir.Path);
                    ImGui.SetDragDropPayload(nameof(String), (&str), (uint)sizeof(UnsafeOldString));
                }
                ImGui.Text(dir.Name);
                ImGui.EndDragDropSource();
            }
        }

        private void DisplayFile(Item file)
        {
            bool isSelected = SelectedFile == file.Path;

            if (ImGui.Selectable(file.Name, isSelected, ImGuiSelectableFlags.DontClosePopups))
            {
                SelectedFile = file.Path;
            }

            ImGui.PushID(file.Path);
            if (ImGui.BeginPopupContextItem(file.Path))
            {
                if (ImGui.MenuItem("Open"))
                {
                    SelectedFile = file.Path;
                    Designer.OpenFile(SelectedFile);
                }

                if (ImGui.MenuItem("Delete"))
                {
                    File.Delete(file.Path);
                    Refresh();
                }

                if (ImGui.MenuItem("Rename"))
                {
                    dialog.File = file.Path;
                    dialog.Show();
                }

                ImGui.EndPopup();
            }
            ImGui.PopID();

            if (ImGui.IsItemClicked(0) && ImGui.IsMouseDoubleClicked(0))
            {
                Designer.OpenFile(SelectedFile);
            }

            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    var str = new UnsafeOldString(file.Path);
                    ImGui.SetDragDropPayload(nameof(String), (&str), (uint)sizeof(UnsafeOldString));
                }
                ImGui.Text(file.Name);
                ImGui.EndDragDropSource();
            }
        }

        private void DisplayContextMenu()
        {
            if (currentDir == null)
            {
                return;
            }

            if (ImGui.BeginPopupContextWindow("AssetExplorerContextMenu"))
            {
                if (ImGui.MenuItem("Home"))
                {
                    GoHome();
                }

                if (ImGui.MenuItem("Refresh"))
                {
                    Refresh();
                }
                if (ImGui.BeginMenu("New"))
                {
                    if (ImGui.MenuItem("Scene"))
                    {
                        SceneSerializer.Serialize(new(), GetNewFilename(Path.Combine(currentDir.FullName, "New Scene.hexlvl")));
                        Refresh();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }

        private static string GetNewFilename(string filename)
        {
            if (!File.Exists(filename))
            {
                return filename;
            }

            string result = filename;

            int i = 0;
            string name = Path.GetFileNameWithoutExtension(result);
            string extension = Path.GetExtension(filename);
            string dir = new(Path.GetDirectoryName(filename));

            while (File.Exists(result))
            {
                i++;
                result = Path.Combine(dir, $"{name} {i}{extension}");
            }

            return result;
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (currentDir == null)
            {
                return;
            }

            if (dialog.Draw())
            {
                Refresh();
            }
            if (currentDir.Exists)
            {
                ImGui.BeginChild("AssetExplorerContent");

                DisplayContextMenu();

                if (parentDir != null && CurrentFolder != Paths.CurrentProjectFolder)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                    if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        SetFolder(parentDir.FullName);
                    }

                    if (ImGui.BeginDragDropTarget())
                    {
                        unsafe
                        {
                            var payload = ImGui.AcceptDragDropPayload(nameof(String));
                            if (!payload.IsNull)
                            {
                                string ft = *(UnsafeOldString*)payload.Data;
                                if (Directory.Exists(ft))
                                {
                                    var fname = Path.GetFileName(ft);
                                    Directory.Move(ft, Path.Combine(Path.Combine("..\\", ft), fname));
                                }
                                else if (File.Exists(ft))
                                {
                                    File.Move(ft, Path.Combine("..\\", ft));
                                }
                            }
                        }
                        ImGui.EndDragDropTarget();
                    }

                    ImGui.PopStyleColor();
                }

                for (int i = 0; i < dirs.Count; i++)
                {
                    DisplayDir(dirs[i]);
                }

                for (int i = 0; i < files.Count; i++)
                {
                    DisplayFile(files[i]);
                }

                ImGui.EndChild();
            }
        }
    }
}
#endif