namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Projects;
    using HexaEngine.Scenes.Serialization;
    using ImGuiNET;
    using System.Collections.Generic;
    using System.Numerics;

    public enum PasteMode
    {
        None = 0,
        Copy,
        Cut
    }

    public class AssetExplorer2 : EditorWindow
    {
        private DirectoryInfo? currentDir;
        private DirectoryInfo? parentDir;
        private readonly RenameFileDialog dialog = new(true);
        private readonly List<Item> files = new();
        private readonly List<Item> dirs = new();
        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();
        private string? CurrentFolder = null;

        private bool showExtensions = false;

        private PasteMode pasteMode;
        private string? pasteTarget;

#pragma warning disable CS0169 // The field 'AssetExplorer.renameItem' is never used
        private Item? renameItem;
#pragma warning restore CS0169 // The field 'AssetExplorer.renameItem' is never used

        private struct Item(string name, string path)
        {
            public string Path = path;
            public string Name = name;
            public string NameNoExtension = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public AssetExplorer2()
        {
            IsShown = true;
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

        protected override string Name => "Assets";

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
                    dirs.Add(new(Path.GetFileName(fse), fse));
                }
                else
                {
                    files.Add(new(Path.GetFileName(fse), fse));
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

        private static void TextCentered(string text)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var textWidth = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);
        }

        private static void TextVHCentered(string text)
        {
            var windowSize = ImGui.GetWindowSize();
            var textSize = ImGui.CalcTextSize(text);

            ImGui.SetCursorPos((windowSize - textSize) * 0.5f);
            ImGui.Text(text);
        }

        private static void ImageCentered(ImTextureID image, Vector2 size)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var textWidth = size.X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Image(image, size);
        }

        private static void CopyDir(string sourceDirName, string destDirName, bool overwrite)
        {
            foreach (var file in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDirName, file);
                var destFile = Path.Combine(destDirName, rel);

                var destFileDirectory = Path.GetDirectoryName(destFile);
                Directory.CreateDirectory(destFileDirectory);

                File.Copy(file, destFile, overwrite);
            }
        }

        private void DisplayDir(Item dir)
        {
            var icon = IconManager.GetIconForDirectory(dir.Path);

            ImGui.BeginChild(dir.Path, new Vector2(72, 86));

            ImageCentered(icon, new(64, 64));
            TextCentered(dir.Name);

            if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(0))
            {
                SetFolder(dir.Path);
                Designer.OpenFile(SelectedFile);
            }

            ImGui.EndChild();

            ImGui.SetItemTooltip(dir.Name);

            ImGui.PushID(dir.Path);
            if (ImGui.BeginPopupContextItem(dir.Path))
            {
                if (ImGui.MenuItem("\xE845 Open"))
                {
                    SetFolder(dir.Path);
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8C6 Cut"))
                {
                    pasteMode = PasteMode.Cut;
                    pasteTarget = dir.Path;
                }

                if (ImGui.MenuItem("\xE8C8 Copy"))
                {
                    pasteMode = PasteMode.Copy;
                    pasteTarget = dir.Path;
                }

                if (ImGui.MenuItem("\xE77F Paste"))
                {
                    Paste(dir.Path);
                }

                if (ImGui.MenuItem("\xE74D Delete"))
                {
                    MessageBox.Show("Delete directory", $"Are you sure you want to delete the directory and all containing files?\n{dir.Path}", null, (x, c) =>
                    {
                        if (x.Result != MessageBoxResult.Yes)
                            return;
                        Directory.Delete(dir.Path, true);
                        Refresh();
                    }, MessageBoxType.YesCancel);
                }

                if (ImGui.MenuItem("\xE8AC Rename"))
                {
                    dialog.File = dir.Path;
                    dialog.Show();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8C8 Copy Full Path"))
                {
                    Clipboard.SetClipboardText(dir.Path);
                }

                if (ImGui.MenuItem("\xE845 Open Folder in Explorer"))
                {
                    Designer.OpenDirectory(dir.Path);
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
            src_flags |= ImGuiDragDropFlags.AcceptNoPreviewTooltip;     // Keep the source displayed as hovered
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

        private void Paste(string dir)
        {
            if (pasteTarget != null)
            {
                if (pasteMode == PasteMode.Cut && Directory.Exists(pasteTarget))
                {
                    var name = Path.GetFileName(pasteTarget);
                    Directory.Move(pasteTarget, Path.Combine(dir, name));
                }
                if (pasteMode == PasteMode.Copy && Directory.Exists(pasteTarget))
                {
                    var name = Path.GetFileName(pasteTarget);
                    CopyDir(pasteTarget, Path.Combine(dir, name), true);
                }
                if (pasteMode == PasteMode.Cut && File.Exists(pasteTarget))
                {
                    var name = Path.GetFileName(pasteTarget);
                    name = Path.Combine(dir, name);
                    name = GetNewFileName(name);
                    File.Move(pasteTarget, name);
                }
                if (pasteMode == PasteMode.Copy && File.Exists(pasteTarget))
                {
                    var name = Path.GetFileName(pasteTarget);
                    name = Path.Combine(dir, name);
                    name = GetNewFileName(name);
                    File.Copy(pasteTarget, name);
                }
            }

            Refresh();
        }

        private void DisplayFile(Item file)
        {
            bool isSelected = SelectedFile == file.Path;

            var icon = IconManager.GetIconForFile(file.Name);

            ImGui.BeginChild(file.Path, new Vector2(72, 86));

            var hovered = ImGui.IsWindowHovered();

            ImageCentered(icon, new(64, 64));
            TextCentered(showExtensions ? file.Name : file.NameNoExtension);

            if (ImGui.IsWindowHovered() && ImGui.IsMouseDoubleClicked(0))
            {
                Designer.OpenFile(file.Path);
            }

            ImGui.EndChild();

            //ImGui.SetItemTooltip(file.Name);

            ImGui.PushID(file.Path);

            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    fixed (char* pt = file.Path)
                    {
                        ImGui.SetDragDropPayload(nameof(String), pt, (nuint)file.Path.Length * sizeof(char));
                    }
                }
                ImGui.Text(file.Name);
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginPopupContextItem(file.Path))
            {
                if (ImGui.MenuItem("\xE845 Open"))
                {
                    SelectedFile = file.Path;
                    Designer.OpenFile(SelectedFile);
                }

                if (ImGui.MenuItem("Open With..."))
                {
                    SelectedFile = file.Path;
                    Designer.OpenFileWith(SelectedFile);
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8C6 Cut"))
                {
                    pasteMode = PasteMode.Cut;
                    pasteTarget = file.Path;
                }

                if (ImGui.MenuItem("\xE8C8 Copy"))
                {
                    pasteMode = PasteMode.Copy;
                    pasteTarget = file.Path;
                }

                if (ImGui.MenuItem("\xE74D Delete"))
                {
                    MessageBox.Show("Delete file", $"Are you sure you want to delete the file?\n{file.Path}", null, (x, c) =>
                    {
                        if (x.Result != MessageBoxResult.Yes)
                            return;
                        File.Delete(file.Path);
                        Refresh();
                    }, MessageBoxType.YesCancel);
                }

                if (ImGui.MenuItem("\xE8AC Rename"))
                {
                    dialog.File = file.Path;
                    dialog.Show();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8C8 Copy Full Path"))
                {
                    Clipboard.SetClipboardText(file.Path);
                }

                if (ImGui.MenuItem("\xE845 Open Containing Folder"))
                {
                    Designer.OpenDirectory(currentDir?.FullName);
                }

                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        private void DisplayContextMenu()
        {
            if (currentDir == null)
            {
                return;
            }

            if (ImGui.BeginPopupContextWindow("AssetExplorerContextMenu"))
            {
                if (ImGui.BeginMenu("Add"))
                {
                    if (ImGui.BeginMenu("New Item"))
                    {
                        if (ImGui.MenuItem("Scene"))
                        {
                            SceneSerializer.Serialize(new(), GetNewFileName(Path.Combine(currentDir.FullName, "New Scene.hexlvl")));
                            Refresh();
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.MenuItem("New Folder"))
                    {
                        Directory.CreateDirectory(GetNewFolderName(Path.Combine(currentDir.FullName, "New Folder")));
                        Refresh();
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Home"))
                {
                    GoHome();
                }

                ImGui.Checkbox("Show Extensions", ref showExtensions);

                if (ImGui.MenuItem("Refresh"))
                {
                    Refresh();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE77F Paste"))
                {
                    Paste(currentDir.FullName);
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8C8 Copy Full Path"))
                {
                    Clipboard.SetClipboardText(currentDir.FullName);
                }

                if (ImGui.MenuItem("\xE845 Open Folder in Explorer"))
                {
                    Designer.OpenDirectory(currentDir.FullName);
                }

                ImGui.EndPopup();
            }
        }

        private static string GetNewFileName(string filename)
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

        private static string GetNewFolderName(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return folder;
            }

            string result = folder;

            int i = 0;
            string name = Path.GetFileNameWithoutExtension(result);
            string dir = new(Path.GetDirectoryName(folder));

            while (Directory.Exists(result))
            {
                i++;
                result = Path.Combine(dir, $"{name} {i}");
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

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows | ImGuiHoveredFlags.RootWindow))
                {
                    if (ImGui.IsMouseClicked((ImGuiMouseButton)3))
                    {
                        TryGoBack();
                    }
                    if (ImGui.IsMouseClicked((ImGuiMouseButton)4))
                    {
                        TryGoForward();
                    }
                }

                DisplayContextMenu();

                var style = ImGui.GetStyle();

                float size = 72 + style.WindowPadding.X;
                var windowSize = ImGui.GetContentRegionAvail();
                float x = 0;
                float y = 0;

                if (parentDir != null && CurrentFolder != Paths.CurrentProjectFolder)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));

                    ImGui.BeginChild("../", new Vector2(72, 86));

                    TextVHCentered("../");

                    if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(0))
                        SetFolder(parentDir.FullName);

                    ImGui.EndChild();

                    ImGui.SameLine();
                    x += size;

                    if (ImGui.BeginDragDropTarget())
                    {
                        unsafe
                        {
                            var payload = ImGui.AcceptDragDropPayload(nameof(String));
                            if (!payload.IsNull)
                            {
                                string ft = new((char*)payload.Data, 0, payload.DataSize);
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
                    x += size;
                    if (x + size < windowSize.X)
                    {
                        ImGui.SameLine();
                    }
                    else
                    {
                        x = 0;
                    }
                }

                for (int i = 0; i < files.Count; i++)
                {
                    DisplayFile(files[i]);
                    x += size;
                    if (x + size < windowSize.X)
                    {
                        ImGui.SameLine();
                    }
                    else
                    {
                        x = 0;
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}