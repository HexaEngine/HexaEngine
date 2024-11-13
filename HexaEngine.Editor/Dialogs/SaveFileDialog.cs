namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using System.Numerics;

    public class SaveFileDialog
    {
        private bool shown;
        private DirectoryInfo currentDir;
        private readonly List<Item> files = new();
        private readonly List<Item> dirs = new();
        public string RootFolder;
        private string currentFolder;
        private string currentFolderBar;
        private string selectedFile = string.Empty;
        public List<string> AllowedExtensions = new();
        public bool OnlyAllowFolders;
        public bool OnlyAllowFilteredExtensions;
        public SaveFileResult Result;

        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();

        private struct Item(string name, string filename, string path)
        {
            public string Path = path;
            public string Filename = filename;
            public string Name = name;
        }

        public SaveFileDialog() : this(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), null)
        {
        }

        public SaveFileDialog(string startingPath) : this(startingPath, null)
        {
        }

        public SaveFileDialog(string? startingPath, string? searchFilter)
        {
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (searchFilter != null)
            {
                if (AllowedExtensions != null)
                {
                    AllowedExtensions.Clear();
                }
                else
                {
                    AllowedExtensions = new List<string>();
                }

                AllowedExtensions.AddRange(searchFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                OnlyAllowFilteredExtensions = true;
            }

            currentDir = new DirectoryInfo(startingPath);
            RootFolder = startingPath;
            CurrentFolder = startingPath;
            OnlyAllowFolders = false;
        }

        public bool Shown => shown;

        public string FullPath
        {
            get => Path.Combine(currentFolder, selectedFile);
            set
            {
                currentFolderBar = currentFolder = Path.GetDirectoryName(value) ?? string.Empty;
                selectedFile = Path.GetFileName(value);
            }
        }

        public string SelectedFile { get => selectedFile; set => selectedFile = value; }

        public string CurrentFolder
        {
            get => currentFolder; set
            {
                currentFolderBar = currentFolder = value;
                Refresh();
            }
        }

        public void Show()
        {
            shown = true;
        }

        public void Hide()
        {
            shown = false;
        }

        public unsafe bool Draw()
        {
            if (!shown)
            {
                return false;
            }

            if (ImGui.Begin("File picker", ref shown, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse))
            {
                ImGui.SetWindowFocus();

                if (ImGui.Button($"{UwU.House}"))
                {
                    CurrentFolder = RootFolder;
                }
                ImGui.SameLine();
                if (ImGui.Button($"{UwU.Backward}"))
                {
                    TryGoBack();
                }
                ImGui.SameLine();
                if (ImGui.Button($"{UwU.Forward}"))
                {
                    TryGoForward();
                }
                ImGui.SameLine();
                if (ImGui.Button($"{UwU.ArrowsRotate}"))
                {
                    Refresh();
                }
                ImGui.SameLine();

                if (ImGui.InputText("Path", ref currentFolderBar, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (Directory.Exists(currentFolderBar))
                    {
                        SetFolder(currentFolderBar);
                    }
                    else
                    {
                        currentFolderBar = currentFolder;
                    }
                }

                float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();

                float widthDrives = 100 + ImGui.GetStyle().ItemSpacing.X * 2;
                Vector2 contentRegion = default;
                ImGui.GetContentRegionAvail(ref contentRegion);
                float width = contentRegion.X - ImGui.GetStyle().ItemSpacing.X - widthDrives;
                if (ImGui.BeginChild(1, new Vector2(widthDrives, -footerHeightToReserve), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    void Display(string? rel, string str)
                    {
                        if (File.Exists(str))
                        {
                            return;
                        }

                        if (ImGui.TreeNodeEx(rel != null ? Path.GetRelativePath(rel, str) : str, ImGuiTreeNodeFlags.OpenOnArrow))
                        {
                            if (Directory.Exists(str))
                            {
                                foreach (var item in GetFileSystemEntries(str))
                                {
                                    Display(str, item);
                                }
                            }

                            ImGui.TreePop();
                        }
                        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                        {
                            SetFolder(str);
                        }
                    }
                    if (ImGui.TreeNodeEx("Computer", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        string profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        foreach (var drive in GetSpecialDirs())
                        {
                            Display(profilePath, drive);
                        }
                        foreach (var drive in GetDrives())
                        {
                            Display(null, drive);
                        }
                        ImGui.TreePop();
                    }
                }
                ImGui.EndChild();

                ImGui.SameLine();
                if (ImGui.BeginChild(2, new Vector2(width, -footerHeightToReserve), ImGuiChildFlags.Borders, 0))
                {
                    if (currentDir.Exists)
                    {
                        if (currentDir.Parent != null)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                            if (ImGui.Selectable("../", false, ImGuiSelectableFlags.NoAutoClosePopups))
                            {
                                SetFolder(currentDir.Parent.FullName);
                            }

                            ImGui.PopStyleColor();
                        }

                        for (int i = 0; i < dirs.Count; i++)
                        {
                            var dir = dirs[i];
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                            if (ImGui.Selectable(dir.Name, false, ImGuiSelectableFlags.NoAutoClosePopups))
                            {
                                SetFolder(dir.Path);
                            }

                            ImGui.PopStyleColor();
                        }

                        for (int i = 0; i < files.Count; i++)
                        {
                            var file = files[i];

                            bool isSelected = selectedFile == file.Path;
                            if (ImGui.Selectable(file.Name, isSelected, ImGuiSelectableFlags.NoAutoClosePopups))
                            {
                                selectedFile = file.Filename;
                            }

                            if (ImGui.IsItemClicked(0) && ImGuiP.IsMouseDoubleClicked(0))
                            {
                                Result = SaveFileResult.Ok;
                                ImGui.EndChild();
                                ImGui.End();
                                shown = false;
                                return true;
                            }
                        }
                    }
                }
                ImGui.EndChild();

                if (ImGui.InputText("Selected", ref selectedFile, 1024))
                {
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    Result = SaveFileResult.Cancel;
                    ImGui.End();
                    shown = false;
                    return true;
                }

                if (OnlyAllowFolders)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Save"))
                    {
                        Result = SaveFileResult.Ok;
                        selectedFile = currentFolder;
                        ImGui.End();
                        shown = false;
                        return true;
                    }
                }
                else if (selectedFile != null)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Open"))
                    {
                        Result = SaveFileResult.Ok;
                        ImGui.End();
                        shown = false;
                        return true;
                    }
                }
            }

            ImGui.End();
            return false;
        }

        public void SetFolder(string path)
        {
            backHistory.Push(currentFolder);
            CurrentFolder = path;
            forwardHistory.Clear();
        }

        public void GoHome()
        {
            CurrentFolder = RootFolder;
        }

        public void TryGoBack()
        {
            if (backHistory.TryPop(out var historyItem))
            {
                forwardHistory.Push(currentFolder);
                CurrentFolder = historyItem;
            }
        }

        public void TryGoForward()
        {
            if (forwardHistory.TryPop(out var historyItem))
            {
                backHistory.Push(currentFolder);
                CurrentFolder = historyItem;
            }
        }

        private static string[] GetDrives()
        {
            return Directory.GetLogicalDrives();
        }

        private static string[] GetSpecialDirs()
        {
            return
            [
                Application.GetFolder(Application.SpecialFolder.Assets),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            ];
        }

        public void Refresh()
        {
            currentDir = new DirectoryInfo(CurrentFolder);
            files.Clear();
            dirs.Clear();

            foreach (var fse in Directory.GetFileSystemEntries(currentFolder, string.Empty))
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
                    dirs.Add(new($"{UwU.Folder} {Path.GetFileName(fse)}", Path.GetFileName(fse), fse));
                }
                else if (!OnlyAllowFolders)
                {
                    if (OnlyAllowFilteredExtensions)
                    {
                        var ext = Path.GetExtension(fse);
                        if (AllowedExtensions.Contains(ext))
                        {
                            files.Add(new($"{UwU.File} {Path.GetFileName(fse)}", Path.GetFileName(fse), fse));
                        }
                    }
                    else
                    {
                        files.Add(new($"{UwU.File} {Path.GetFileName(fse)}", Path.GetFileName(fse), fse));
                    }
                }
            }
        }

        private List<string> GetFileSystemEntries(string fullName)
        {
            var files = new List<string>();
            var dirs = new List<string>();
            foreach (var fse in Directory.GetFileSystemEntries(fullName, string.Empty))
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
                    dirs.Add(fse);
                }
                else if (!OnlyAllowFolders)
                {
                    if (OnlyAllowFilteredExtensions)
                    {
                        var ext = Path.GetExtension(fse);
                        if (AllowedExtensions.Contains(ext))
                        {
                            files.Add(fse);
                        }
                    }
                    else
                    {
                        files.Add(fse);
                    }
                }
            }

            var ret = new List<string>(dirs);
            ret.AddRange(files);

            return ret;
        }
    }
}