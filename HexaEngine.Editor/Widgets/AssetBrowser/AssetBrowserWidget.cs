﻿namespace HexaEngine.Editor.Widgets.AssetBrowser
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using Hexa.NET.Utilities;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Extensions;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Scenes.Serialization;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using OpenFileDialog = Hexa.NET.ImGui.Widgets.Dialogs.OpenFileDialog;
    using PopupManager = PopupManager;

    // TODO: Filter and Search
    // TODO: File Preview
    // TODO: Sort and Order by
    // TODO: Favorites and Bookmarks
    // TODO: Batch operations
    // TODO: Version Control
    // TODO: Asset Importing
    // TODO: Metadata
    // TODO: Automatic refresh

    /// <summary>
    /// A editor widget for managing assets.
    /// </summary>
    public class AssetBrowserWidget : EditorWindow
    {
        private static readonly ConfigKey config = Config.Global.GetOrCreateKey("Editor").GetOrCreateKey("Asset Browser");
        private DirectoryInfo? currentDir;
        private DirectoryInfo? parentDir;
        private AssetDirectory rootDir;
        private readonly List<AssetItem> files = [];
        private readonly List<AssetItem> dirs = [];
        private readonly HashSet<Guid> groups = [];
        private readonly HashSet<Guid> openGroups = [];
        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();
        private string? CurrentFolder = null;

        private readonly Lock refreshLock = new();

        private bool showExtensions = false;
        private bool showHidden = false;

        private AssetExplorerIconSize iconSize = AssetExplorerIconSize.Medium;

        private Vector2 chipSize = new(86, 92);
        private Vector2 imageSize = new(64, 64);

        private PasteMode pasteMode;
        private Vector2 windowPos;
        private Vector2 windowSize;
        private bool isWindowHovered;

        public AssetBrowserWidget()
        {
            IsShown = true;
            Application.MainWindow.DropFile += DropFile;
            SourceAssetsDatabase.Changed += FileSystemChanged;
            Refresh(true);
            currentDir = null;
            parentDir = currentDir?.Parent;
            ProjectManager.ProjectLoaded += ProjectLoaded;
            IconSize = config.GetOrAddValue("Icon Size", AssetExplorerIconSize.Medium);
            ShowExtensions = config.GetOrAddValue("Show Extensions", false);
            Flags |= ImGuiWindowFlags.MenuBar;
        }

        protected override void DisposeCore()
        {
            Application.MainWindow.DropFile -= DropFile;
            SourceAssetsDatabase.Changed -= FileSystemChanged;
            ProjectManager.ProjectLoaded -= ProjectLoaded;
        }

        private void FileSystemChanged(FileSystemEventArgs obj)
        {
            Refresh(!File.Exists(obj.FullPath));
        }

        private void ProjectLoaded(HexaProject? obj)
        {
            RefreshDirs();
            SetFolder(ProjectManager.CurrentProjectAssetsFolder);
        }

        protected override string Name { get; } = $"{UwU.LinesLeaning} Asset Browser";

        public AssetExplorerIconSize IconSize
        {
            get => iconSize; set
            {
                iconSize = value;
                config.SetValue("Icon Size", value);
                Config.SaveGlobal();
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
                Config.SaveGlobal();
            }
        }

        public bool ShowHidden
        {
            get => showHidden;
            set
            {
                showHidden = value;
                config.SetValue("Show Hidden", showHidden);
                Config.SaveGlobal();
                Refresh(true);
            }
        }

        public void RefreshDirs()
        {
            if (ProjectManager.CurrentProjectFolder == null)
            {
                return;
            }

            lock (this)
            {
                rootDir = TraverseDir(ProjectManager.CurrentProjectFolder);
            }
        }

        private static AssetDirectory TraverseDir(string dir)
        {
            AssetDirectory directory = new(Path.GetFileName(dir), dir);
            foreach (string subDir in Directory.EnumerateDirectories(dir))
            {
                if (SourceAssetsDatabase.IsIgnored(subDir))
                {
                    continue;
                }

                AssetDirectory subDirectory = TraverseDir(subDir);
                directory.SubDirs.Add(subDirectory);
            }
            return directory;
        }

        public void Refresh(bool all)
        {
            if (all)
            {
                RefreshDirs();
            }

            lock (refreshLock)
            {
                files.Clear();
                dirs.Clear();
                if (CurrentFolder == null)
                {
                    return;
                }

                currentDir = new(CurrentFolder);
                parentDir = currentDir?.Parent;

                foreach (var fse in Directory.GetFileSystemEntries(CurrentFolder))
                {
                    bool isDir = Directory.Exists(fse);
                    bool isFile = File.Exists(fse);

                    if (!isDir && !isFile)
                    {
                        continue;
                    }

                    if (SourceAssetsDatabase.IsIgnored(fse))
                    {
                        continue;
                    }

                    if (fse.EndsWith(".meta") && !showHidden)
                    {
                        continue;
                    }

                    if (Directory.Exists(fse))
                    {
                        dirs.Add(new(Path.GetFileName(fse), fse, null, null));
                    }
                    else
                    {
                        var metadata = SourceAssetsDatabase.GetMetadata(fse);
                        Ref<Texture2D>? thumbnail = null;
                        if (metadata != null)
                        {
                            SourceAssetsDatabase.ThumbnailCache.TryGet(metadata.Guid, out thumbnail);
                            if (metadata.GroupGuid != default)
                            {
                                groups.Add(metadata.GroupGuid);
                            }
                        }
                        files.Add(new(Path.GetFileName(fse), fse, metadata, thumbnail));
                    }
                }

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file.Metadata == null || file.Metadata.GroupGuid == default)
                    {
                        continue;
                    }

                    Guid groupGuid = file.Metadata.GroupGuid;

                    for (int j = 0; j < files.Count; j++)
                    {
                        var item = files[j];
                        if (item.Metadata != null && item.Metadata.Guid == groupGuid)
                        {
                            item.GroupItems.Add(file);
                            files.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                }

                files.Sort(new AssetItemGroupComparer(groups));
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
            Refresh(false);
        }

        public void GoHome()
        {
            CurrentFolder = Paths.CurrentProjectFolder;
            Refresh(false);
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
                Refresh(false);
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
                Refresh(false);
            }
        }

        private static void CopyDir(string sourceDirName, string destDirName, bool overwrite)
        {
            foreach (var file in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDirName, file);
                var destFile = Path.Combine(destDirName, rel);

                var destFileDirectory = Path.GetDirectoryName(destFile);
                Directory.CreateDirectory(destFileDirectory!);

                File.Copy(file, destFile, overwrite);
            }
        }

        private void DisplayDir(AssetItem dir)
        {
            ImGui.BeginChild(dir.Path, chipSize);

            var startPos = ImGui.GetCursorPos();

            var icon = IconManager.GetIconForDirectory(dir.Path);
            icon.ImageCenteredH(imageSize);
            TextHelper.TextCenteredH(dir.Name);

            if (ImGui.IsWindowHovered())
            {
                if (ImGuiP.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    AssetFileInfo info = new(dir.Path, dir.Metadata);
                    if (ImGuiP.IsKeyDown(ImGuiKey.LeftCtrl))
                    {
                        SelectionCollection.Global.AddSelection(info);
                    }
                    else
                    {
                        SelectionCollection.Global.AddOverwriteSelection(info);
                    }

                    if (ImGuiP.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        SetFolder(dir.Path);
                    }
                }
            }

            ImGui.SetCursorPos(startPos);
            ImGui.InvisibleButton(dir.Name, chipSize);

            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    fixed (char* pt = dir.Path)
                    {
                        ImGui.SetDragDropPayload(nameof(AssetFileInfo), pt, (nuint)dir.Path.Length * sizeof(char));
                    }
                }
                for (int i = 0; i < SelectionCollection.Global.Count; i++)
                {
                    if (SelectionCollection.Global[i] is AssetFileInfo fileInfo)
                    {
                        ImGui.Text(fileInfo.Name);
                    }
                }

                ImGui.EndDragDropSource();
            }

            ImGui.EndChild();

            ImGui.SetItemTooltip(dir.Name);

            DirectoryContextMenu(dir);

            if (ImGui.BeginDragDropTarget())
            {
                DragDropTarget(dir.Path);
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
                    // TODO: Remove string from here imgui copies the string anyway, else it would produce memory leaks.
                    var str = new StdWString(dir.Path);
                    ImGui.SetDragDropPayload(nameof(String), &str, (uint)sizeof(StdWString));
                }
                ImGui.Text(dir.Name);
                ImGui.EndDragDropSource();
            }
        }

        private void DirectoryContextMenu(AssetItem dir)
        {
            ImGui.PushID(dir.Path);
            if (ImGui.BeginPopupContextItem(dir.Path))
            {
                if (ImGui.MenuItem($"{UwU.OpenFile} Open"))
                {
                    SetFolder(dir.Path);
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Cut} Cut"))
                {
                    pasteMode = PasteMode.Cut;
                }

                if (ImGui.MenuItem($"{UwU.Copy} Copy"))
                {
                    pasteMode = PasteMode.Copy;
                }

                if (ImGui.MenuItem($"{UwU.Paste} Paste"))
                {
                    Paste(dir.Path);
                }

                if (ImGui.MenuItem($"{UwU.Trash} Delete"))
                {
                    MessageBox.Show("Delete directory", $"Are you sure you want to delete the directory and all containing files?\n{dir.Path}", dir.Path, (x, c) =>
                    {
                        if (x.Result != MessageBoxResult.Yes)
                        {
                            return;
                        }

                        Directory.Delete((string)c!, true);
                        Refresh(true);
                    }, MessageBoxType.YesCancel);
                }

                if (ImGui.MenuItem($"{UwU.Rename} Rename"))
                {
                    RenameDialog dialog = new(dir.Path, RenameDialogFlags.Default | RenameDialogFlags.NoAutomaticMove);
                    dialog.Show(RenameDialogCallback);
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Copy} Copy Full Path"))
                {
                    Clipboard.SetText(dir.Path);
                }

                if (ImGui.MenuItem($"{UwU.FolderOpen} Open Folder in Explorer"))
                {
                    Designer.OpenDirectory(dir.Path);
                }

                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        public void Paste(string dir)
        {
            foreach (var item in SelectionCollection.Global)
            {
                if (item is AssetFileInfo file && File.Exists(file.Path))
                {
                    if (pasteMode == PasteMode.Cut)
                    {
                        var name = Path.GetFileName(file.Path);
                        name = Path.Combine(dir, name);
                        name = GetNewFileName(name);
                        File.Move(file.Path, name);
                    }
                    if (pasteMode == PasteMode.Copy)
                    {
                        var name = Path.GetFileName(file.Path);
                        name = Path.Combine(dir, name);
                        name = GetNewFileName(name);
                        File.Copy(file.Path, name);
                    }
                }
                if (item is AssetDirectoryInfo directory && Directory.Exists(directory.Path))
                {
                    if (pasteMode == PasteMode.Cut)
                    {
                        var name = Path.GetFileName(directory.Path);
                        Directory.Move(directory.Path, Path.Combine(dir, name));
                    }
                    if (pasteMode == PasteMode.Copy)
                    {
                        var name = Path.GetFileName(directory.Path);
                        CopyDir(directory.Path, Path.Combine(dir, name), true);
                    }
                }
            }

            Refresh(true);
        }

        private unsafe bool DisplayFileBody(AssetItem file, Guid guid)
        {
            bool isSelected = SelectionCollection.Global.Contains(new AssetFileInfo(file.Path, file.Name, null));

            ImGui.BeginChild(file.Path, chipSize, ImGuiWindowFlags.NoScrollbar);

            var startPos = ImGui.GetCursorPos();

            Vector2 min = ImGui.GetCursorScreenPos();
            Vector2 max = min + chipSize;
            ImRect rect = new(min, max);
            uint id = ImGui.GetID(file.Path);

            ImGuiP.ItemSize(rect);
            if (!ImGuiP.ItemAdd(rect, id))
            {
                ImGui.EndChild();
                return false;
            }

            ImDrawList* drawList = ImGui.GetWindowDrawList();

            if (isSelected)
            {
                uint col = ImGui.GetColorU32(ImGuiCol.TextSelectedBg);
                drawList->AddRectFilled(min, max, col);
            }

            Vector2 imgMin = new(min.X + (chipSize.X - imageSize.X) * 0.5f, min.Y);
            Vector2 imgMax = imgMin + imageSize;

            if (file.Thumbnail != null && !file.Thumbnail.IsNull)
            {
                drawList->AddImage((ulong)file.Thumbnail.Value!.SRV!.NativePointer, imgMin, imgMax);
            }
            else
            {
                var icon = IconManager.GetIconForFile(file.Name);
                drawList->AddImage(icon, imgMin, imgMax, icon.UVStart, icon.UVEnd);
            }

            string text = showExtensions ? file.Name : file.NameNoExtension;
            var textWidth = ImGui.CalcTextSize(text).X;
            Vector2 textMin = new(min.X + (chipSize.X - textWidth) * 0.5f, imgMax.Y);
            drawList->AddText(textMin, ImGui.GetColorU32(ImGuiCol.Text), text);

            TooltipHelper.Tooltip(file.Name);

            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    fixed (char* pt = file.Path)
                    {
                        ImGui.SetDragDropPayload(nameof(AssetFileInfo), pt, (nuint)file.Path.Length * sizeof(char));
                    }
                }
                for (int i = 0; i < SelectionCollection.Global.Count; i++)
                {
                    if (SelectionCollection.Global[i] is AssetFileInfo fileInfo)
                    {
                        ImGui.Text(fileInfo.Name);
                    }
                }

                ImGui.EndDragDropSource();
            }

            if (groups.Contains(guid))
            {
                ImGui.SetCursorPos(new(chipSize.X - 32, chipSize.Y / 2 - 16));
                if (!openGroups.Contains(guid))
                {
                    if (ImGui.Button(">"u8))
                    {
                        openGroups.Add(guid);
                    }
                }
                else
                {
                    if (ImGui.Button("<"u8))
                    {
                        openGroups.Remove(guid);
                    }
                }
            }
            bool isHovered;
            bool held;
            bool clicked = ImGuiP.ButtonBehavior(rect, id, &isHovered, &held);

            ImGui.EndChild();

            FileContextMenu(file);
            HandleInput(clicked, isHovered, file);

            return isHovered;
        }

        private unsafe void HandleInput(bool clicked, bool isHovered, AssetItem file)
        {
            if (!isHovered)
            {
                return;
            }

            if (clicked)
            {
                AssetFileInfo info = file;
                bool shift = ImGuiP.IsKeyDown(ImGuiKey.LeftShift);
                bool ctrl = ImGuiP.IsKeyDown(ImGuiKey.LeftCtrl);

                if (shift)
                {
                    if (SelectionCollection.Global.Count > 0 && SelectionCollection.Global[0] is AssetFileInfo item)
                    {
                        var start = files.IndexOf(new AssetItem(item.Path));
                        var end = files.IndexOf(file);
                        if (start == -1 || end == -1) return;
                        if (start > end) (start, end) = (end, start);
                        SelectionCollection.Global.ClearSelection();
                        for (int i = start; i <= end; i++)
                        {
                            var currentFile = files[i];
                            AssetFileInfo currentInfo = new(currentFile.Path, currentFile.Metadata);
                            SelectionCollection.Global.AddSelection(currentInfo);
                        }
                    }
                    else
                    {
                        SelectionCollection.Global.AddSelection(info);
                    }
                }
                else if (ctrl)
                {
                    SelectionCollection.Global.AddSelection(info);
                }
                else
                {
                    SelectionCollection.Global.AddOverwriteSelection(info);
                }

                if (file.GroupItems.Count > 0)
                {
                    foreach (var item in file.GroupItems)
                    {
                        AssetFileInfo subInfo = item;
                        SelectionCollection.Global.AddSelection(subInfo);
                    }
                }
            }

            if (ImGuiP.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                Designer.OpenFile(file.Path);
            }

            if (ImGuiP.IsMouseClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup(file.Path);

                if (!ImGui.GetIO().KeyCtrl)
                {
                    AssetFileInfo info = new(file.Path, file.Metadata);
                    SelectionCollection.Global.AddSelection(info);
                }
            }
        }

        private void FileContextMenu(AssetItem file)
        {
            if (ImGui.BeginPopupContextItem(file.Path, ImGuiPopupFlags.MouseButtonRight))
            {
                if (ImGui.MenuItem($"{UwU.OpenFile} Open"))
                {
                    Designer.OpenFile(file.Path);
                }

                if (ImGui.MenuItem("Open With..."))
                {
                    Designer.OpenFileWith(file.Path);
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Cut} Cut"))
                {
                    pasteMode = PasteMode.Cut;
                }

                if (ImGui.MenuItem($"{UwU.Copy} Copy"))
                {
                    pasteMode = PasteMode.Copy;
                }

                if (ImGui.MenuItem($"{UwU.Trash} Delete"))
                {
                    MessageBox.Show("Delete file", $"Are you sure you want to delete the file(s)?", this, (x, c) =>
                    {
                        AssetBrowserWidget browser = (AssetBrowserWidget)c!;
                        if (x.Result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        for (int i = 0; i < SelectionCollection.Global.Count; i++)
                        {
                            var selection = SelectionCollection.Global[i];
                            if (selection is AssetFileInfo fileInfo)
                            {
                                SourceAssetsDatabase.Delete(fileInfo.Path, DeleteBehavior.DeleteChildren);
                            }
                        }

                        browser.Refresh(false);
                    }, MessageBoxType.YesCancel);
                }

                if (ImGui.MenuItem($"{UwU.Rename} Rename"))
                {
                    RenameDialog dialog = new(file.Path, RenameDialogFlags.Default | RenameDialogFlags.NoAutomaticMove);
                    dialog.Show(RenameDialogCallback);
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Copy} Copy Full Path"))
                {
                    Clipboard.SetText(file.Path);
                }

                if (ImGui.MenuItem($"{UwU.FolderOpen} Open Containing Folder"))
                {
                    Designer.OpenDirectory(currentDir?.FullName);
                }

                ImGui.EndPopup();
            }
        }

        private void RenameDialogCallback(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || sender is not RenameDialog dialog) return;
            if (dialog.IsDirectory)
            {
                SourceAssetsDatabase.MoveFolder(dialog.SourcePath, dialog.DestinationPath!);
            }
            else
            {
                SourceAssetsDatabase.Move(dialog.SourcePath, dialog.DestinationPath!);
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
                if (ImGui.BeginMenu($"{UwU.SquarePlus} Add"))
                {
                    if (ImGui.MenuItem($"{UwU.SquarePlus} New Item..."))
                    {
                    }

                    if (ImGui.MenuItem($"{UwU.SquarePlus} New Folder"))
                    {
                        Directory.CreateDirectory(GetNewFolderName(Path.Combine(currentDir.FullName, "New Folder")));
                        Refresh(true);
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem($"{UwU.FileImport} Import Item..."))
                    {
                    }

                    if (ImGui.MenuItem($"{UwU.FileImport} Import Folder..."))
                    {
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Scene"))
                    {
                        var path = GetNewFileName(Path.Combine(currentDir.FullName, "New Scene.hexlvl"));
                        SceneSerializer.Serialize(new(), path);
                        Refresh(false);
                    }

                    if (ImGui.MenuItem("Prefab"))
                    {
                        var path = GetNewFileName(Path.Combine(currentDir.FullName, "New Prefab.prefab"));
                        PrefabSerializer.Serialize(new(), path);
                        Refresh(false);
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.House} Home"))
                {
                    GoHome();
                }

                if (ImGui.MenuItem($"{UwU.ArrowsRotate} Refresh"))
                {
                    Refresh(true);
                }

                ImGui.Separator();

                if (ImGui.BeginMenu($"{UwU.Gear} Settings"))
                {
                    DrawSettings();

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Paste} Paste"))
                {
                    Paste(currentDir.FullName);
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"{UwU.Copy} Copy Full Path"))
                {
                    Clipboard.SetText(currentDir.FullName);
                }

                if (ImGui.MenuItem($"{UwU.FolderOpen} Open Folder in Explorer"))
                {
                    Designer.OpenDirectory(currentDir.FullName);
                }

                ImGui.EndPopup();
            }
        }

        private void DrawSettings()
        {
            if (ComboEnumHelper<AssetExplorerIconSize>.Combo("Icon Size", ref iconSize))
            {
                IconSize = iconSize;
            }

            if (ImGui.Checkbox("Show Extensions"u8, ref showExtensions))
            {
                ShowExtensions = showExtensions;
            }

            if (ImGui.Checkbox("Show Hidden"u8, ref showHidden))
            {
                ShowHidden = showHidden;
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
            windowPos = ImGui.GetWindowPos();
            windowSize = ImGui.GetWindowSize();

            isWindowHovered = ImGui.IsWindowHovered();
            DrawMenuBar();

            if (currentDir == null)
            {
                return;
            }

            ImGui.BeginTable("", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable);
            ImGui.TableSetupColumn("", 200f);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.BeginChild("AssetExplorerSidePanel"u8);

            void DisplayNode(AssetDirectory directory)
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;

                if (directory.SubDirs.Count == 0)
                {
                    flags |= ImGuiTreeNodeFlags.Leaf;
                }

                if (directory == rootDir)
                {
                    flags |= ImGuiTreeNodeFlags.DefaultOpen;
                }

                if (directory.Path == CurrentFolder)
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }

                bool isOpen = ImGui.TreeNodeEx(directory.Name, flags);

                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    SetFolder(directory.Path);
                }

                if (ImGui.BeginDragDropTarget())
                {
                    DragDropTarget(directory.Path);
                    ImGui.EndDragDropTarget();
                }

                if (isOpen)
                {
                    for (int i = 0; i < directory.SubDirs.Count; i++)
                    {
                        DisplayNode(directory.SubDirs[i]);
                    }

                    ImGui.TreePop();
                }
            }

            DisplayNode(rootDir);

            ImGui.EndChild();

            ImGui.TableSetColumnIndex(1);

            if (currentDir.Exists)
            {
                ImGui.BeginChild("AssetExplorerContent"u8);

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows | ImGuiHoveredFlags.RootWindow))
                {
                    if (ImGuiP.IsMouseClicked((ImGuiMouseButton)3))
                    {
                        TryGoBack();
                    }
                    if (ImGuiP.IsMouseClicked((ImGuiMouseButton)4))
                    {
                        TryGoForward();
                    }
                }

                DisplayContextMenu();

                var style = ImGui.GetStyle();

                float size = chipSize.X + style.WindowPadding.X;
                var windowSize = ImGui.GetContentRegionAvail();
                float x = 0;

                bool isContentHovered = false;

                if (refreshLock.TryEnter(0))
                {
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
                        isContentHovered |= DisplayFile(size, windowSize, ref x, files[i]);
                    }

                    refreshLock.Exit();
                }

                if (!isContentHovered && ImGui.IsWindowHovered() && ImGuiP.IsMouseClicked(0))
                {
                    SelectionCollection.Global.Clear();
                }

                ImGui.EndChild();
            }

            ImGui.EndTable();
        }

        private unsafe void DragDropTarget(string directory)
        {
            var payload = ImGui.AcceptDragDropPayload(nameof(AssetFileInfo));
            if (!payload.IsNull)
            {
                for (int i = 0; i < SelectionCollection.Global.Count; i++)
                {
                    if (SelectionCollection.Global[i] is AssetFileInfo fileInfo)
                    {
                        string file = fileInfo.Path;
                        if (Directory.Exists(file))
                        {
                            SourceAssetsDatabase.MoveFolder(file, Path.Combine(directory, Path.GetFileName(fileInfo.Path)));
                        }
                        else if (File.Exists(file))
                        {
                            SourceAssetsDatabase.Move(file, Path.Combine(directory, Path.GetFileName(fileInfo.Path)));
                        }
                    }
                }

                Refresh(true);
            }
        }

        private bool DisplayFile(float size, Vector2 windowSize, ref float x, AssetItem file)
        {
            Guid guid = default;

            if (file.Metadata != null)
            {
                guid = file.Metadata.Guid;
            }

            bool result = DisplayFileBody(file, guid);

            x += size;
            if (x + size < windowSize.X)
            {
                ImGui.SameLine();
            }
            else
            {
                x = 0;
            }

            if (openGroups.Contains(guid))
            {
                for (int j = 0; j < file.GroupItems.Count; j++)
                {
                    result |= DisplayFile(size, windowSize, ref x, file.GroupItems[j]);
                }
            }

            return result;
        }

        private void DropFile(object? sender, Core.Windows.Events.DropFileEventArgs e)
        {
            Vector2 pos = new(e.X, e.Y);

            if (pos.X < windowPos.X || pos.Y < windowPos.Y || pos.X > windowPos.X + windowSize.X || pos.Y > windowPos.Y + windowSize.Y)
            {
                return;
            }

            var file = e.GetString();

            ImportFileAsync(file);
        }

        private void ImportFileAsync(string? file)
        {
            if (file == null || currentDir == null)
            {
                return;
            }

            Task.Factory.StartNew(async args =>
            {
                if (args is not (string file, DirectoryInfo currentDir))
                {
                    return;
                }
                var popup = PopupManager.Show(new ImportProgressModal("Importing asset(s) ...", "Please wait, importing asset(s) ..."));
                try
                {
                    await SourceAssetsDatabase.ImportFileAsync(file, currentDir.FullName, null, popup);
                }
                finally
                {
                    popup.Dispose();
                }
            }, (file, currentDir));
        }

        private void ImportFilesAsync(IReadOnlyList<string> files)
        {
            if (files == null || currentDir == null || files.Count == 0)
            {
                return;
            }

            Task.Factory.StartNew(async args =>
            {
                if (args is not (IReadOnlyList<string> files, DirectoryInfo currentDir))
                {
                    return;
                }

                foreach (var file in files)
                {
                    var popup = PopupManager.Show(new ImportProgressModal("Importing asset(s) ...", "Please wait, importing asset(s) ..."));
                    try
                    {
                        await SourceAssetsDatabase.ImportFileAsync(file, currentDir.FullName, null, popup);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        popup.Dispose();
                    }
                }
            }, (files, currentDir));
        }

        private unsafe void DrawMenuBar()
        {
            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);
            if (ImGui.BeginMenuBar())
            {
                // something here causes gc pressue needs investigation.
                if (ImGui.BeginMenu(builder.BuildLabel(UwU.Gear)))
                {
                    DrawSettings();
                    ImGui.EndMenu();
                }

                if (ImGui.Button(builder.BuildLabel(UwU.FileImport, " Import")))
                {
                    OpenFileDialog openFileDialog = new();
                    openFileDialog.AllowMultipleSelection = true;
                    openFileDialog.Show(ImportCallback);
                }

                ImGui.EndMenuBar();
            }
        }

        private void ImportCallback(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok) return;

            if (sender is OpenFileDialog importFileDialog)
            {
                ImportFilesAsync(importFileDialog.Selection);
            }
        }
    }
}