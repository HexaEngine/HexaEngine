namespace HexaEngine.Editor.ImagePainter
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter.Exporters;
    using System;
    using System.Numerics;

    public class ImageExporter : Modal
    {
        private static readonly TexFileFormat[] formats = Enum.GetValues<TexFileFormat>();
        private static readonly string[] formatNames = Enum.GetNames<TexFileFormat>();
        private BaseExporter exporter;
        private bool isExporterOpen;
        private TexFileFormat format;
        private IScratchImage? image;

        private DirectoryInfo currentDir;
        private readonly List<Item> files = new();
        private readonly List<Item> dirs = new();
        public string RootFolder;
        private string currentFolder;
        private string selectedFile = string.Empty;
        public List<string> AllowedExtensions = new();
        public bool OnlyAllowFolders;
        public bool OnlyAllowFilteredExtensions;
        public SaveFileResult Result;

        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();
        private readonly IGraphicsDevice device;

        private struct Item
        {
            public string Path;
            public string Filename;
            public string Name;

            public Item(string name, string filename, string path)
            {
                Name = name;
                Filename = filename;
                Path = path;
            }
        }

        public ImageExporter(IGraphicsDevice device)
        {
            string startingPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                {
                    startingPath = AppContext.BaseDirectory;
                }
            }

            currentDir = new DirectoryInfo(startingPath);
            RootFolder = startingPath;
            CurrentFolder = startingPath;
            OnlyAllowFolders = false;
            this.device = device;
        }

        public override string Name => "Export Image";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.None;

        public IScratchImage? Image { get => image; set => image = value; }

        public string FullPath => Path.Combine(currentFolder, selectedFile);

        public string SelectedFile { get => selectedFile; set => selectedFile = value; }

        public string CurrentFolder
        {
            get => currentFolder; set
            {
                currentFolder = value;
                Refresh();
            }
        }

        public override void Reset()
        {
            image?.Dispose();
            image = null;
            isExporterOpen = false;
            exporter = null;
        }

        protected override unsafe void DrawContent()
        {
            if (isExporterOpen)
            {
                if (exporter.DrawContent(device))
                {
                    Close();
                    Reset();
                }
                return;
            }
            if (ImGui.Button("\xe80f"))
            {
                CurrentFolder = RootFolder;
            }
            ImGui.SameLine();
            if (ImGui.Button("\xe72b"))
            {
                TryGoBack();
            }
            ImGui.SameLine();
            if (ImGui.Button("\xe72a"))
            {
                TryGoForward();
            }
            ImGui.SameLine();
            if (ImGui.Button("\xe72c"))
            {
                Refresh();
            }
            ImGui.SameLine();
            ImGui.InputText("Path", ref currentFolder, 1024);

            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing() * 2;

            float widthDrives = 100 + ImGui.GetStyle().ItemSpacing.X * 2;
            float width = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X - widthDrives;
            if (ImGui.BeginChild(1, new Vector2(widthDrives, -footerHeightToReserve), false, ImGuiWindowFlags.HorizontalScrollbar))
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
            if (ImGui.BeginChild(2, new Vector2(width, -footerHeightToReserve), true, 0))
            {
                if (currentDir.Parent != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                    if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        SetFolder(currentDir.Parent.FullName);
                    }

                    ImGui.PopStyleColor();
                }

                for (int i = 0; i < dirs.Count; i++)
                {
                    var dir = dirs[i];
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                    if (ImGui.Selectable(dir.Name, false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        SetFolder(dir.Path);
                    }

                    ImGui.PopStyleColor();
                }

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];

                    bool isSelected = selectedFile == file.Path;
                    if (ImGui.Selectable(file.Name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                    {
                        selectedFile = file.Filename;
                    }

                    if (ImGui.IsItemClicked(0) && ImGui.IsMouseDoubleClicked(0))
                    {
                        Result = SaveFileResult.Ok;
                        Export();
                    }
                }
            }
            ImGui.EndChild();

            ImGui.BeginChild(3);

            var index = Array.IndexOf(formats, format);
            if (ImGui.Combo("Format", ref index, formatNames, formatNames.Length))
            {
                format = formats[index];
            }

            if (ImGui.InputText("Selected", ref selectedFile, 1024))
            {
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                Result = SaveFileResult.Cancel;
                Close();
            }

            if (OnlyAllowFolders)
            {
                ImGui.SameLine();
                if (ImGui.Button("Export"))
                {
                    Result = SaveFileResult.Ok;
                    selectedFile = currentFolder;
                    Export();
                }
            }
            else if (selectedFile != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Export"))
                {
                    Result = SaveFileResult.Ok;
                    Export();
                }
            }
            ImGui.EndChild();
        }

        public void Export()
        {
            switch (format)
            {
                case TexFileFormat.DDS:
                    exporter = new DDSExporter();
                    isExporterOpen = true;
                    break;

                case TexFileFormat.RAW:
                    exporter = new RAWExporter();
                    isExporterOpen = true;
                    break;

                default:
                    image?.SaveToFile(FullPath, format, 0);
                    Close();
                    Reset();
                    break;
            }
            exporter.Image = Image;
            exporter.Path = FullPath;
        }

        public void SetFolder(string path)
        {
            backHistory.Push(currentFolder);
            currentFolder = path;
            forwardHistory.Clear();
        }

        public void GoHome()
        {
            currentFolder = RootFolder;
        }

        public void TryGoBack()
        {
            if (backHistory.TryPop(out var historyItem))
            {
                forwardHistory.Push(currentFolder);
                currentFolder = historyItem;
            }
        }

        public void TryGoForward()
        {
            if (forwardHistory.TryPop(out var historyItem))
            {
                backHistory.Push(currentFolder);
                currentFolder = historyItem;
            }
        }

        private static string[] GetDrives()
        {
            return Directory.GetLogicalDrives();
        }

        private static string[] GetSpecialDirs()
        {
            return new string[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            };
        }

        public void Refresh()
        {
            currentDir = new DirectoryInfo(currentFolder);
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
                    dirs.Add(new("\xe8b7" + Path.GetFileName(fse), Path.GetFileName(fse), fse));
                }
                else if (!OnlyAllowFolders)
                {
                    if (OnlyAllowFilteredExtensions)
                    {
                        var ext = Path.GetExtension(fse);
                        if (AllowedExtensions.Contains(ext))
                        {
                            files.Add(new("\xe8a5" + Path.GetFileName(fse), Path.GetFileName(fse), fse));
                        }
                    }
                    else
                    {
                        files.Add(new("\xe8a5" + Path.GetFileName(fse), Path.GetFileName(fse), fse));
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