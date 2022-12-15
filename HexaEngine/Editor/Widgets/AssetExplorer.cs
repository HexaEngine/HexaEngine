namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.IO;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading.Tasks;

    // TODO: Cache entries for folder mode and add project view mode.
    public class AssetExplorer : ImGuiWindow
    {
        private DirectoryInfo currentDir;
        private readonly List<Item> files = new();
        private readonly List<Item> dirs = new();
        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();
        private string CurrentFolder = Paths.CurrentProjectFolder;
        private Task? task;
        private readonly AssimpSceneLoader loader = new();

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
            IsShown = true;
            Refresh();
            currentDir = new(CurrentFolder);
        }

        public string? SelectedFile { get; private set; }

        protected override string Name => "Assets";

        public void Refresh()
        {
            currentDir = new(CurrentFolder);
            files.Clear();
            dirs.Clear();

            foreach (var fse in Directory.GetFileSystemEntries(CurrentFolder, string.Empty))
            {
                if (File.GetAttributes(fse).HasFlag(FileAttributes.System))
                    continue;
                if (File.GetAttributes(fse).HasFlag(FileAttributes.Hidden))
                    continue;
                if (File.GetAttributes(fse).HasFlag(FileAttributes.Device))
                    continue;
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

        public void SetFolder(string path)
        {
            backHistory.Push(CurrentFolder);
            CurrentFolder = path;
            forwardHistory.Clear();
            Refresh();
        }

        public void GoHome()
        {
            CurrentFolder = Paths.CurrentAssetsPath;
            Refresh();
        }

        public void TryGoBack()
        {
            if (backHistory.TryPop(out var historyItem))
            {
                forwardHistory.Push(CurrentFolder);
                CurrentFolder = historyItem;
                Refresh();
            }
        }

        public void TryGoForward()
        {
            if (forwardHistory.TryPop(out var historyItem))
            {
                backHistory.Push(CurrentFolder);
                CurrentFolder = historyItem;
                Refresh();
            }
        }

        public void OpenFile()
        {
            if ((task == null || task.IsCompleted) && SelectedFile != null)
            {
                var extension = Path.GetExtension(SelectedFile);
                if (extension == ".glb")
                {
                    task = loader.OpenAsync(SelectedFile);
                }
                if (extension == ".gltf")
                {
                    task = loader.OpenAsync(SelectedFile);
                }
                if (extension == ".dae")
                {
                    task = loader.OpenAsync(SelectedFile);
                }
                if (extension == ".obj")
                {
                    task = loader.OpenAsync(SelectedFile);
                }
            }
        }

        private void DisplayDir(Item dir)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
            if (ImGui.Selectable(dir.Name, false, ImGuiSelectableFlags.DontClosePopups))
                SetFolder(dir.Path);
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
                    if (payload.NativePtr != null)
                    {
                        string ft = *(UnsafeString*)payload.Data;
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
                    var str = new UnsafeString(dir.Path);
                    ImGui.SetDragDropPayload(nameof(String), (nint)(&str), (uint)sizeof(UnsafeString));
                }
                ImGui.Text(dir.Name);
                ImGui.EndDragDropSource();
            }
        }

        private void DisplayFile(Item file)
        {
            bool isSelected = SelectedFile == file.Path;
            if (ImGui.Selectable(file.Name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                SelectedFile = file.Path;

            ImGui.PushID(file.Path);
            if (ImGui.BeginPopupContextItem(file.Path))
            {
                if (ImGui.MenuItem("Open"))
                {
                    SelectedFile = file.Path;
                    OpenFile();
                }
                if (ImGui.MenuItem("Delete"))
                {
                    File.Delete(file.Path);
                }

                ImGui.EndPopup();
            }
            ImGui.PopID();

            if (ImGui.IsItemClicked(0) && ImGui.IsMouseDoubleClicked(0))
            {
                OpenFile();
            }

            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    var str = new UnsafeString(file.Path);
                    ImGui.SetDragDropPayload(nameof(String), (nint)(&str), (uint)sizeof(UnsafeString));
                }
                ImGui.Text(file.Name);
                ImGui.EndDragDropSource();
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (currentDir.Exists)
            {
                ImGui.BeginChild("AssetExplorerContent");

                if (ImGui.BeginPopupContextWindow("AssetExplorerContextMenu"))
                {
                    if (ImGui.MenuItem("Refresh"))
                    {
                        Refresh();
                    }

                    ImGui.EndPopup();
                }

                if (currentDir.Parent != null && CurrentFolder != Paths.CurrentProjectFolder)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                    if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                        SetFolder(currentDir.Parent.FullName);

                    if (ImGui.BeginDragDropTarget())
                    {
                        unsafe
                        {
                            var payload = ImGui.AcceptDragDropPayload(nameof(String));
                            if (payload.NativePtr != null)
                            {
                                string ft = *(UnsafeString*)payload.Data;
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

                for (int i = 0; i < files.Count; i++)
                {
                    DisplayFile(files[i]);
                }

                for (int i = 0; i < dirs.Count; i++)
                {
                    DisplayDir(dirs[i]);
                }
                ImGui.EndChild();
            }
        }
    }
}