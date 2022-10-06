namespace HexaEngine.Editor
{
    using HexaEngine.Core.IO;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public static class AssetExplorer
    {
        private static string CurrentFolder = Paths.CurrentAssetsPath;
        private static bool isShown;
        private static Task? task;

        public static readonly Stack<string> backHistory = new();

        private static readonly Stack<string> forwardHistory = new();

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static string? SelectedFile { get; private set; }

        public static void Draw()
        {
            if (!ImGui.Begin("Assets", ref isShown, ImGuiWindowFlags.MenuBar))
            {
                ImGui.End();
                return;
            }

            var di = new DirectoryInfo(CurrentFolder);
            if (di.Exists)
            {
                if (di.Parent != null && CurrentFolder != Paths.CurrentAssetsPath)
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
                            if ((task == null || task.IsCompleted) && SelectedFile != null)
                            {
                                var extension = Path.GetExtension(SelectedFile);
                                if (extension == ".glb")
                                {
                                    task = AssimpSceneLoader.OpenAsync(SelectedFile);
                                }
                                if (extension == ".gltf")
                                {
                                    task = AssimpSceneLoader.OpenAsync(SelectedFile);
                                }
                                if (extension == ".dae")
                                {
                                    task = AssimpSceneLoader.OpenAsync(SelectedFile);
                                }
                            }
                        }
                    }
                }
            }

            ImGui.End();
        }

        private static List<string> GetFileSystemEntries(string fullName)
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
                else
                {
                    files.Add(fse);
                }
            }

            var ret = new List<string>(dirs);
            ret.AddRange(files);

            return ret;
        }

        public static void SetFolder(string path)
        {
            backHistory.Push(CurrentFolder);
            CurrentFolder = path;
            forwardHistory.Clear();
        }

        public static void GoHome()
        {
            CurrentFolder = Paths.CurrentAssetsPath;
        }

        public static void TryGoBack()
        {
            if (backHistory.TryPop(out var historyItem))
            {
                forwardHistory.Push(CurrentFolder);
                CurrentFolder = historyItem;
            }
        }

        public static void TryGoForward()
        {
            if (forwardHistory.TryPop(out var historyItem))
            {
                backHistory.Push(CurrentFolder);
                CurrentFolder = historyItem;
            }
        }
    }
}