﻿using ImGuiNET;
using System.Numerics;

namespace HexaEngine.Editor.Dialogs
{
    public class SaveFileDialog
    {
        private bool shown;
        public string RootFolder;
        public string CurrentFolder;
        public string SelectedFile = string.Empty;
        public List<string> AllowedExtensions = new();
        public bool OnlyAllowFolders;
        public bool OnlyAllowFilteredExtensions;
        public SaveFileResult Result;

        public readonly Stack<string> backHistory = new();

        private readonly Stack<string> forwardHistory = new();

        public SaveFileDialog()
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
                    startingPath = AppContext.BaseDirectory;
            }

            RootFolder = startingPath;
            CurrentFolder = startingPath;
            OnlyAllowFolders = false;
        }

        public SaveFileDialog(string startingPath)
        {
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                    startingPath = AppContext.BaseDirectory;
            }

            RootFolder = startingPath;
            CurrentFolder = startingPath;
            OnlyAllowFolders = false;
        }

        public SaveFileDialog(string startingPath, string? searchFilter = null)
        {
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                    startingPath = AppContext.BaseDirectory;
            }

            RootFolder = startingPath;
            CurrentFolder = startingPath;
            OnlyAllowFolders = false;

            if (searchFilter != null)
            {
                if (AllowedExtensions != null)
                    AllowedExtensions.Clear();
                else
                    AllowedExtensions = new List<string>();

                AllowedExtensions.AddRange(searchFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public bool Shown => shown;

        public void Show()
        {
            shown = true;
        }

        public void Hide()
        {
            shown = false;
        }

        public bool Draw()
        {
            if (!shown) return false;
            if (ImGui.Begin("File picker", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse))
            {
                ImGui.SetWindowFocus();

                if (ImGui.Button("Home"))
                {
                    CurrentFolder = RootFolder;
                }
                ImGui.SameLine();
                if (ImGui.Button("<"))
                {
                    TryGoBack();
                }
                ImGui.SameLine();
                if (ImGui.Button(">"))
                {
                    TryGoForward();
                }
                ImGui.SameLine();
                ImGui.InputText("Path", ref CurrentFolder, 1024);

                float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();

                float widthDrives = 100 + ImGui.GetStyle().ItemSpacing.X * 2;
                float width = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X - widthDrives;
                if (ImGui.BeginChild(1, new Vector2(widthDrives, -footerHeightToReserve), false, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    void Display(string? rel, string str)
                    {
                        if (File.Exists(str))
                            return;

                        if (ImGui.TreeNodeEx(rel != null ? Path.GetRelativePath(rel, str) : str, ImGuiTreeNodeFlags.OpenOnArrow))
                        {
                            if (Directory.Exists(str))
                                foreach (var item in GetFileSystemEntries(str))
                                {
                                    Display(str, item);
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
                    var di = new DirectoryInfo(CurrentFolder);
                    if (di.Exists)
                    {
                        if (di.Parent != null)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                            if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                                SetFolder(di.Parent.FullName);

                            ImGui.PopStyleColor();
                        }

                        var fileSystemEntries = GetFileSystemEntries(di.FullName);
                        foreach (var fse in fileSystemEntries)
                        {
                            if (Directory.Exists(fse))
                            {
                                var name = Path.GetFileName(fse);
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                                if (ImGui.Selectable("\xe8b7" + name, false, ImGuiSelectableFlags.DontClosePopups))
                                    SetFolder(fse);
                                ImGui.PopStyleColor();
                            }
                            else
                            {
                                var name = Path.GetFileName(fse);
                                bool isSelected = SelectedFile == fse;
                                if (ImGui.Selectable("\xe8a5" + name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                                    SelectedFile = fse;

                                if (ImGui.IsMouseDoubleClicked(0))
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
                }
                ImGui.EndChild();

                if (ImGui.InputText("Selected", ref SelectedFile, 1024))
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
                        SelectedFile = CurrentFolder;
                        ImGui.End();
                        shown = false;
                        return true;
                    }
                }
                else if (SelectedFile != null)
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
            backHistory.Push(CurrentFolder);
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
                forwardHistory.Push(CurrentFolder);
                CurrentFolder = historyItem;
            }
        }

        public void TryGoForward()
        {
            if (forwardHistory.TryPop(out var historyItem))
            {
                backHistory.Push(CurrentFolder);
                CurrentFolder = historyItem;
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

        private List<string> GetFileSystemEntries(string fullName)
        {
            var files = new List<string>();
            var dirs = new List<string>();
            foreach (var fse in Directory.GetFileSystemEntries(fullName, string.Empty))
            {
                if (File.GetAttributes(fse).HasFlag(FileAttributes.System))
                    continue;
                if (File.GetAttributes(fse).HasFlag(FileAttributes.Hidden))
                    continue;
                if (File.GetAttributes(fse).HasFlag(FileAttributes.Device))
                    continue;
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
                            files.Add(fse);
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