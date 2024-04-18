namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Editor.Projects;
    using Octokit;
    using System;
    using System.Numerics;

    public class LauncherWindow : Modal
    {
        private static readonly OpenFileDialog filePicker = new(Environment.CurrentDirectory);
        private static readonly SaveFileDialog fileSaver = new(Environment.CurrentDirectory);
        private static Action<OpenFileResult, string>? filePickerCallback;
        private static Action<SaveFileResult, SaveFileDialog>? fileSaverCallback;
        private string searchString = string.Empty;
        private HistoryEntry historyEntry;
        private bool first = false;

        private bool createProjectDialog;

        public LauncherWindow()
        {
        }

        public override string Name => "Open Project";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar;

        public override unsafe void Draw()
        {
            if (!shown || signalClose)
            {
                base.Draw();
                return;
            }

            if (filePicker.Draw())
            {
                filePickerCallback?.Invoke(filePicker.Result, filePicker.FullPath);
            }

            if (fileSaver.Draw())
            {
                fileSaverCallback?.Invoke(fileSaver.Result, fileSaver);
            }

            if (ImGui.BeginPopupModal("DeleteNonExistingProject"))
            {
                ImGui.Text("The selected Project doesn't exist, do you want to remove it from the History?");

                if (ImGui.Button("Yes"))
                {
                    ProjectHistory.RemoveEntryByPath(historyEntry.Path);
                    ImGui.CloseCurrentPopup();
                    Show();
                }
                if (ImGui.Button("No"))
                {
                    ImGui.CloseCurrentPopup();
                    Show();
                }

                ImGui.EndPopup();
            }

            Vector2 main_viewport_pos = ImGui.GetMainViewport().Pos;
            Vector2 main_viewport_size = ImGui.GetMainViewport().Size;
            ImGui.SetNextWindowPos(main_viewport_pos);
            ImGui.SetNextWindowSize(main_viewport_size);
            ImGui.SetNextWindowBgAlpha(0.9f);
            ImGui.Begin("Overlay", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
            ImGui.End();

            if (first)
            {
                Vector2 size = new(800, 500);
                Vector2 mainViewportPos = ImGui.GetMainViewport().Pos;
                Vector2 s = ImGui.GetPlatformIO().Monitors.Data[0].MainSize;

                ImGui.SetNextWindowSize(size);
                ImGui.SetNextWindowPos(mainViewportPos + (s / 2 - size / 2));
                first = false;
            }

            base.Draw();
        }

        public override void Show()
        {
            first = true;
            CreateProjectDialogReset();
            base.Show();
        }

        protected override unsafe void DrawContent()
        {
            ImGui.Separator();

            Vector2 pos = ImGui.GetCursorPos();
            Vector2 padding = ImGui.GetStyle().CellPadding;
            Vector2 spacing = ImGui.GetStyle().ItemSpacing;
            float lineHeight = ImGui.GetTextLineHeight();

            const float widthSide = 300;
            Vector2 avail = ImGui.GetContentRegionAvail();
            Vector2 entrySize = new(avail.X - widthSide, ImGui.GetTextLineHeight() * 2 + padding.Y * 2 + spacing.Y);
            Vector2 trueEntrySize = entrySize - new Vector2(ImGui.GetStyle().IndentSpacing, 0);

            Icon icon = IconManager.GetIconByName("Logo") ?? throw new();

            if (createProjectDialog)
            {
                CreateProjectDialog(icon, avail);
                return;
            }

            var entries = ProjectHistory.Entries;

            Vector2 entryChildSize = new(entrySize.X, avail.Y);
            ImGui.BeginChild("Entries", entryChildSize);

            ImGui.InputTextWithHint("##SearchBar", "Search ...", ref searchString, 1024);

            if (entries.Any(x => x.Pinned) && ImGui.TreeNodeEx("Pinned", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    HistoryEntry entry = entries[i];

                    if (!entry.Pinned)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchString) && (!entry.Name.Contains(searchString) || !entry.Path.Contains(searchString)))
                    {
                        continue;
                    }

                    DisplayEntry(entry, icon, padding, spacing, lineHeight, trueEntrySize);
                }
                ImGui.TreePop();
            }

            int activeNode = -1;
            bool open = false;
            foreach (var entry in entries.OrderByDescending(x => x.LastAccess))
            {
                if (!string.IsNullOrWhiteSpace(searchString) && (!entry.Name.Contains(searchString) || !entry.Path.Contains(searchString)))
                {
                    continue;
                }

                if (entry.LastAccess.Date == DateTime.UtcNow.Date)
                {
                    if (activeNode != 0)
                    {
                        if (activeNode != -1 && open)
                        {
                            ImGui.TreePop();
                        }

                        open = ImGui.TreeNodeEx("Today", ImGuiTreeNodeFlags.DefaultOpen);
                        activeNode = 0;
                    }
                }
                else if (entry.LastAccess.Date == DateTime.UtcNow.Date.AddDays(-1))
                {
                    if (activeNode != 1)
                    {
                        if (activeNode != -1 && open)
                        {
                            ImGui.TreePop();
                        }

                        open = ImGui.TreeNodeEx("Yesterday", ImGuiTreeNodeFlags.DefaultOpen);
                        activeNode = 1;
                    }
                }
                else if (entry.LastAccess.Date > DateTime.UtcNow.Date.AddDays(-7))
                {
                    if (activeNode != 2)
                    {
                        if (activeNode != -1 && open)
                        {
                            ImGui.TreePop();
                        }

                        open = ImGui.TreeNodeEx("A week ago", ImGuiTreeNodeFlags.DefaultOpen);
                        activeNode = 2;
                    }
                }
                else if (entry.LastAccess.Date > DateTime.UtcNow.Date.AddMonths(-1))
                {
                    if (activeNode != 3)
                    {
                        if (activeNode != -1 && open)
                        {
                            ImGui.TreePop();
                        }

                        open = ImGui.TreeNodeEx("A month ago", ImGuiTreeNodeFlags.DefaultOpen);
                        activeNode = 3;
                    }
                }
                else if (activeNode != 4)
                {
                    if (activeNode != -1 && open)
                    {
                        ImGui.TreePop();
                    }

                    open = ImGui.TreeNode("Older");
                    activeNode = 4;
                }

                if (open)
                {
                    DisplayEntry(entry, icon, padding, spacing, lineHeight, trueEntrySize);
                }
            }

            if (activeNode != -1 && open)
            {
                ImGui.TreePop();
            }

            ImGui.EndChild();

            Vector2 childSize = new(widthSide - padding.X, avail.Y);
            ImGui.SetCursorPos(pos + new Vector2(entrySize.X + padding.X, 0));
            ImGui.BeginChild("Child", childSize);

            if (ImGui.Button("\xE710 New Project", new(childSize.X, 50)))
            {
                createProjectDialog = true;
            }
            if (ImGui.Button("\xF78B Open Project", new(childSize.X, 50)))
            {
                filePicker.AllowedExtensions.Add(".hexproj");
                filePicker.OnlyAllowFilteredExtensions = true;
                filePickerCallback = (e, r) =>
                {
                    if (e == OpenFileResult.Ok)
                    {
                        if (File.Exists(r))
                        {
                            ProjectManager.Load(r);
                            Close();
                        }
                    }

                    filePicker.AllowedExtensions.Clear();
                    filePicker.OnlyAllowFilteredExtensions = false;
                };
                filePicker.Show();
            }
            if (ImGui.Button("\xE896 Clone Project", new(childSize.X, 50)))
            {
                // TODO: Implement git clone.
            }

            ImGui.EndChild();
        }

        private string newProjectName = "New Project";
        private string newProjectPath = "";
        private bool canCreateProject = false;
        private string? canCreateFailReason;

        private static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        private static bool IsValidProjectDir(string path)
        {
            return !Directory.Exists(path) || IsDirectoryEmpty(path);
        }

        private void CreateProjectDialog(Icon icon, Vector2 avail)
        {
            const float footerHeight = 50;
            avail.Y -= footerHeight;
            ImGui.BeginChild("Content", avail);

            icon.Image(new(48));
            ImGui.SameLine();
            ImGui.Text("Create a new Project");

            ImGui.Dummy(new(0, 20));

            ImGui.Indent(48);

            ImGui.Text("Project Name:");

            if (ImGui.InputText("##ProjectName", ref newProjectName, 1024))
            {
                newProjectPath = Path.Combine(EditorConfig.Default.ProjectsFolder, newProjectName);
                canCreateProject = IsValidProjectDir(newProjectPath) && !string.IsNullOrWhiteSpace(newProjectName);
                canCreateFailReason = canCreateProject ? null : string.IsNullOrWhiteSpace(newProjectName) ? "\xE7BA Project name cannot be empty." : "\xE7BA Project already exists.";
            }

            ImGui.TextDisabled(newProjectPath);

            if (!canCreateProject)
            {
                ImGui.Dummy(new(0, 20));
                ImGui.TextColored(new(1, 0, 0, 1), canCreateFailReason ?? string.Empty);
            }

            ImGui.Unindent();

            ImGui.EndChild();

            ImGui.BeginTable("#Table", 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("");

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);

            if (ImGui.Button("Cancel"))
            {
                CreateProjectDialogReset();
            }
            ImGui.SameLine();

            ImGui.BeginDisabled(!canCreateProject);

            if (ImGui.Button("Create"))
            {
                Directory.CreateDirectory(newProjectPath);
                ProjectManager.Create(newProjectPath);
                CreateProjectDialogReset();
                Close();
            }
            ImGui.EndDisabled();

            ImGui.EndTable();
        }

        private void CreateProjectDialogReset()
        {
            newProjectName = "New Project";
            newProjectPath = Path.Combine(EditorConfig.Default.ProjectsFolder ?? string.Empty, newProjectName);

            string newName = newProjectName;
            int i = 1;
            while (Directory.Exists(newProjectPath))
            {
                newName = $"{newProjectName} {i++}";
                newProjectPath = Path.Combine(EditorConfig.Default.ProjectsFolder ?? string.Empty, newName);
            }
            newProjectName = newName;
            canCreateProject = true;

            createProjectDialog = false;
        }

        private void DisplayEntry(HistoryEntry entry, Icon icon, Vector2 padding, Vector2 spacing, float lineHeight, Vector2 entrySize)
        {
            Vector2 pos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new(entrySize.X, pos.Y + padding.Y));

            if (ImGui.SmallButton(entry.Pinned ? $"\xE77A##{entry.Path}" : $"\xE718##{entry.Path}"))
            {
                if (entry.Pinned)
                {
                    ProjectHistory.Unpin(entry.Path);
                }
                else
                {
                    ProjectHistory.Pin(entry.Path);
                }
            }

            ImGui.SetCursorPos(pos);

            if (ImGui.Button($"##{entry.Path}", entrySize))
            {
                if (File.Exists(entry.Path))
                {
                    ProjectManager.Load(entry.Path);
                    Close();
                }
                else
                {
                    historyEntry = entry;
                    ImGui.OpenPopup("DeleteNonExistingProject");
                }
            }

            DisplayEntryContextMenu(entry);

            ImGui.SetCursorPos(pos + padding);

            Vector2 imageSize = new(entrySize.Y - padding.Y * 2);

            icon.Image(imageSize);

            Vector2 nextPos = new(pos.X + padding.X + imageSize.X + spacing.X, pos.Y + padding.Y);

            ImGui.SetCursorPos(nextPos);

            ImGui.Text(entry.Name);

            float size = ImGui.CalcTextSize(entry.LastAccessString).X;

            ImGui.SetCursorPos(new(entrySize.X - size, nextPos.Y));
            ImGui.Text(entry.LastAccessString);

            nextPos.Y += spacing.Y + lineHeight;
            nextPos.X += 5;

            ImGui.SetCursorPos(nextPos);

            ImGui.TextDisabled(entry.Path);

            ImGui.SetCursorPosY(pos.Y + entrySize.Y + spacing.Y);
        }

        private static void DisplayEntryContextMenu(HistoryEntry entry)
        {
            if (ImGui.BeginPopupContextItem($"##{entry.Path}"))
            {
                if (ImGui.MenuItem($"\xE711 Remove from List"))
                {
                    ProjectHistory.RemoveEntryByPath(entry.Path);
                }
                if (!entry.Pinned && ImGui.MenuItem("\xE718 Pin"))
                {
                    ProjectHistory.Pin(entry.Path);
                }
                if (entry.Pinned && ImGui.MenuItem("\xE77A Unpin"))
                {
                    ProjectHistory.Unpin(entry.Path);
                }
                if (ImGui.MenuItem("\xE8C8 Copy Path"))
                {
                    Clipboard.SetClipboardText(entry.Path);
                }

                ImGui.EndPopup();
            }
        }

        public override void Reset()
        {
            CreateProjectDialogReset();
        }
    }
}