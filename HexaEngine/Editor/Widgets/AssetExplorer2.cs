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

    public enum AssetExplorerDisplayMode
    {
        Minimal,
        Pretty,
    }

    public enum AssetExplorerIconSize
    {
        Small,
        Medium,
        Large,
    }

    /*
     TODOS:
        Filter and Search
        File Preview
        Sort and Order by
        Favorites and Bookmarks
        Batch operations
        Version Control
        Asset Importing
        Metadata
        Automatic refresh
     */

    public class AssetExplorer2 : EditorWindow
    {
        private static readonly ConfigKey config = Config.Global.GetOrCreateKey("Editor").GetOrCreateKey("Asset Explorer");
        private DirectoryInfo? currentDir;
        private DirectoryInfo? parentDir;
        private readonly RenameFileDialog dialog = new(true);
        private readonly List<Item> files = new();
        private readonly List<Item> dirs = new();
        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();
        private string? CurrentFolder = null;

        private bool showExtensions = false;

        private AssetExplorerDisplayMode displayMode = AssetExplorerDisplayMode.Pretty;
        private AssetExplorerIconSize iconSize = AssetExplorerIconSize.Medium;

        private Vector2 chipSize = new(72, 86);
        private Vector2 imageSize = new(64, 64);

        private PasteMode pasteMode;
        private string? pasteTarget;

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
            ProjectManager.ProjectChanged += ProjectLoaded;
            DisplayMode = config.GetOrAddValue("Display Mode", AssetExplorerDisplayMode.Pretty);
            IconSize = config.GetOrAddValue("Icon Size", AssetExplorerIconSize.Medium);
            ShowExtensions = config.GetOrAddValue("Show Extensions", false);
        }

        private void ProjectLoaded(HexaProject? obj)
        {
            SetFolder(ProjectManager.CurrentProjectAssetsFolder);
        }

        public string? SelectedFile { get; private set; }

        protected override string Name => "Assets";

        public AssetExplorerDisplayMode DisplayMode
        {
            get => displayMode;
            set
            {
                displayMode = value;
                config.SetValue("Display Mode", value);
                Config.Global.Save();
            }
        }

        public AssetExplorerIconSize IconSize
        {
            get => iconSize; set
            {
                iconSize = value;
                config.SetValue("Icon Size", value);
                Config.Global.Save();
                switch (value)
                {
                    case AssetExplorerIconSize.Small:
                        imageSize = new(32, 32);
                        chipSize = new(40, 52);
                        break;

                    case AssetExplorerIconSize.Medium:
                        imageSize = new(64, 64);
                        chipSize = new(72, 86);
                        break;

                    case AssetExplorerIconSize.Large:
                        imageSize = new(86, 86);
                        chipSize = new(94, 108);
                        break;
                }
            }
        }

        public bool ShowExtensions
        {
            get => showExtensions;
            set
            {
                showExtensions = value;
                config.SetValue("Show Extensions", showExtensions);
                Config.Global.Save();
            }
        }

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
            switch (displayMode)
            {
                case AssetExplorerDisplayMode.Minimal:
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));

                    if (ImGui.Selectable($"\xe8b7{dir.Name}", false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        SetFolder(dir.Path);
                    }

                    ImGui.PopStyleColor();
                    break;

                case AssetExplorerDisplayMode.Pretty:
                    ImGui.BeginChild(dir.Path, chipSize);

                    var icon = IconManager.GetIconForDirectory(dir.Path);
                    ImageHelper.ImageCenteredH(icon, imageSize);
                    TextHelper.TextCenteredH(dir.Name);

                    if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(0))
                    {
                        SetFolder(dir.Path);
                        Designer.OpenFile(SelectedFile);
                    }

                    ImGui.EndChild();

                    ImGui.SetItemTooltip(dir.Name);
                    break;
            }

            DirectoryContextMenu(dir);

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

        private void DirectoryContextMenu(Item dir)
        {
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
        }

        public void Paste(string dir)
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
            switch (displayMode)
            {
                case AssetExplorerDisplayMode.Minimal:
                    bool isSelected = SelectedFile == file.Path;

                    if (ImGui.Selectable($"\xe8a5{(showExtensions ? file.Name : file.NameNoExtension)}", isSelected, ImGuiSelectableFlags.DontClosePopups))
                    {
                        SelectedFile = file.Path;
                    }
                    break;

                case AssetExplorerDisplayMode.Pretty:

                    ImGui.BeginChild(file.Path, chipSize);

                    var icon = IconManager.GetIconForFile(file.Name);

                    var hovered = ImGui.IsWindowHovered();

                    ImageHelper.ImageCenteredH(icon, imageSize);
                    TextHelper.TextCenteredH(showExtensions ? file.Name : file.NameNoExtension);

                    if (ImGui.IsWindowHovered() && ImGui.IsMouseDoubleClicked(0))
                    {
                        Designer.OpenFile(file.Path);
                    }

                    ImGui.EndChild();
                    break;
            }

            FileContextMenu(file);

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
        }

        private void FileContextMenu(Item file)
        {
            ImGui.PushID(file.Path);

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
                if (ImGui.BeginMenu("\xE948 Add"))
                {
                    if (ImGui.MenuItem("\xE948 New Item..."))
                    {
                    }

                    if (ImGui.MenuItem("\xE948 New Folder"))
                    {
                        Directory.CreateDirectory(GetNewFolderName(Path.Combine(currentDir.FullName, "New Folder")));
                        Refresh();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("\xEDAD Import Item..."))
                    {
                    }

                    if (ImGui.MenuItem("\xEDAD Import Folder..."))
                    {
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Scene"))
                    {
                        SceneSerializer.Serialize(new(), GetNewFileName(Path.Combine(currentDir.FullName, "New Scene.hexlvl")));
                        Refresh();
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE80F Home"))
                {
                    GoHome();
                }

                if (ImGui.MenuItem("\xE72C Refresh"))
                {
                    Refresh();
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("\xE713 Settings"))
                {
                    if (ComboEnumHelper<AssetExplorerDisplayMode>.Combo("Display Mode", ref displayMode))
                    {
                        DisplayMode = displayMode;
                    }

                    if (ComboEnumHelper<AssetExplorerIconSize>.Combo("Icon Size", ref iconSize))
                    {
                        IconSize = iconSize;
                    }

                    if (ImGui.Checkbox("Show Extensions", ref showExtensions))
                    {
                        ShowExtensions = showExtensions;
                    }

                    ImGui.EndMenu();
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

                float size = chipSize.X + style.WindowPadding.X;
                var windowSize = ImGui.GetContentRegionAvail();
                float x = 0;
                float y = 0;

                if (parentDir != null && CurrentFolder != Paths.CurrentProjectFolder)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));

                    ImGui.BeginChild("../", new Vector2(72, 86));

                    TextHelper.TextCenteredVH("../");

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
                    if (x + size < windowSize.X && displayMode == AssetExplorerDisplayMode.Pretty)
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
                    if (x + size < windowSize.X && displayMode == AssetExplorerDisplayMode.Pretty)
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