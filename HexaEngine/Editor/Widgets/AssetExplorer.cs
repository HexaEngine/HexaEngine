﻿namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Runtime.Intrinsics.X86;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class AssetExplorer : Widget
    {
        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();
        private string CurrentFolder = Paths.CurrentAssetsPath;
        private Task? task;

        public AssetExplorer()
        {
            IsShown = true;
        }

        public string? SelectedFile { get; private set; }

        private unsafe struct StringPtr
        {
            public StringPtr(string str)
            {
                fixed (char* strPtr = str)
                {
                    Ptr = strPtr;
                }
                Length = str.Length;
            }

            public char* Ptr;
            public int Length;

            public static implicit operator string(StringPtr ptr)
            {
                return new(new Span<char>(ptr.Ptr, ptr.Length));
            }
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

        public void SetFolder(string path)
        {
            backHistory.Push(CurrentFolder);
            CurrentFolder = path;
            forwardHistory.Clear();
        }

        public void GoHome()
        {
            CurrentFolder = Paths.CurrentAssetsPath;
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

        public void OpenFile()
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

        public override void Init(IGraphicsDevice device)
        {
        }

        public override void Draw(IGraphicsContext context)
        {
            if (!IsShown) return;
            if (!ImGui.Begin("Assets", ref IsShown, ImGuiWindowFlags.MenuBar))
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

                    if (ImGui.BeginDragDropTarget())
                    {
                        unsafe
                        {
                            var payload = ImGui.AcceptDragDropPayload(nameof(String));
                            if (payload.NativePtr != null)
                            {
                                string ft = *(StringPtr*)payload.Data;
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

                        ImGui.PushID(fse);
                        if (ImGui.BeginPopupContextItem(fse))
                        {
                            if (ImGui.MenuItem("Open"))
                            {
                                SetFolder(fse);
                            }
                            if (ImGui.MenuItem("Delete"))
                            {
                                Directory.Delete(fse, true);
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
                                    string ft = *(StringPtr*)payload.Data;
                                    if (Directory.Exists(ft))
                                    {
                                        var fname = Path.GetFileName(ft);
                                        Directory.Move(ft, Path.Combine(fse, fname));
                                    }
                                    else if (File.Exists(ft))
                                    {
                                        File.Move(ft, fse);
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
                                var str = new StringPtr(fse);
                                ImGui.SetDragDropPayload(nameof(String), (nint)(&str), (uint)sizeof(StringPtr));
                            }
                            ImGui.Text(name);
                            ImGui.EndDragDropSource();
                        }
                    }
                    else
                    {
                        var name = Path.GetFileName(fse);
                        bool isSelected = SelectedFile == fse;
                        if (ImGui.Selectable("\xe8a5" + name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                            SelectedFile = fse;

                        ImGui.PushID(fse);
                        if (ImGui.BeginPopupContextItem(fse))
                        {
                            if (ImGui.MenuItem("Open"))
                            {
                                SelectedFile = fse;
                                OpenFile();
                            }
                            if (ImGui.MenuItem("Delete"))
                            {
                                File.Delete(fse);
                            }

                            ImGui.EndPopup();
                        }
                        ImGui.PopID();

                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            OpenFile();
                        }

                        if (ImGui.BeginDragDropSource())
                        {
                            unsafe
                            {
                                var str = new StringPtr(fse);
                                ImGui.SetDragDropPayload(nameof(String), (nint)(&str), (uint)sizeof(StringPtr));
                            }
                            ImGui.Text(name);
                            ImGui.EndDragDropSource();
                        }
                    }
                }
            }

            ImGui.End();
        }

        public override void DrawMenu()
        {
            if (ImGui.MenuItem("Assets"))
            {
                IsShown = true;
            }
        }

        public override void Dispose()
        {
        }
    }
}