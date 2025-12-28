namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Extensions;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Graphics.Renderers;
    using System;
    using System.Numerics;

    public class LauncherWindow : EditorWindow
    {
        private string searchString = string.Empty;
        private HistoryEntry historyEntry;

        private bool createProjectDialog;

        public LauncherWindow()
        {
            isEmbedded = true;
            Flags = ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar;
            Show();
        }

        protected override string Name { get; } = "Open Project";

        public override unsafe void DrawWindow(IGraphicsContext context)
        {/*
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
            }*/

            base.DrawWindow(context);
        }

        public override void Show()
        {
            CreateProjectDialogReset();
            base.Show();
        }

        public override void Close()
        {
            Designer.InitEditor();
            base.Close();
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
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

            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);

            var entries = ProjectHistory.Entries;
            var pinned = ProjectHistory.Pinned;

            Vector2 entryChildSize = new(entrySize.X, avail.Y);
            ImGui.BeginChild("Entries"u8, entryChildSize);

            ImGui.InputTextWithHint("##SearchBar", "Search ...", ref searchString, 1024);

            if (ImGui.TreeNodeEx("Pinned", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < pinned.Count; i++)
                {
                    HistoryEntry entry = pinned[i];

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
            for (int i = 0; i < entries.Count; i++)
            {
                HistoryEntry entry = entries[i];
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

                        open = ImGui.TreeNodeEx("Today"u8, ImGuiTreeNodeFlags.DefaultOpen);
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

                        open = ImGui.TreeNodeEx("Yesterday"u8, ImGuiTreeNodeFlags.DefaultOpen);
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

                        open = ImGui.TreeNodeEx("A week ago"u8, ImGuiTreeNodeFlags.DefaultOpen);
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

                        open = ImGui.TreeNodeEx("A month ago"u8, ImGuiTreeNodeFlags.DefaultOpen);
                        activeNode = 3;
                    }
                }
                else if (activeNode != 4)
                {
                    if (activeNode != -1 && open)
                    {
                        ImGui.TreePop();
                    }

                    open = ImGui.TreeNode("Older"u8);
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
            ImGui.Dummy(new(1));
            ImGui.EndChild();

            Vector2 childSize = new(widthSide - padding.X, avail.Y);
            ImGui.SetCursorPos(pos + new Vector2(entrySize.X + padding.X, 0));
            ImGui.BeginChild("Child"u8, childSize);

            if (ImGui.Button(builder.BuildLabel(UwU.SquarePlus, " New Project"), new(childSize.X, 50)))
            {
                createProjectDialog = true;
            }
            if (ImGui.Button(builder.BuildLabel(UwU.MagnifyingGlass, " Open Project"), new(childSize.X, 50)))
            {
                OpenFileDialog dialog = new();
                dialog.AllowedExtensions.Add(".hexproj");
                dialog.OnlyAllowFilteredExtensions = true;
                dialog.Show(OpenProjectCallback);
            }
            if (ImGui.Button(builder.BuildLabel(UwU.Clone, " Clone Project"), new(childSize.X, 50)))
            {
                // TODO: Implement git clone.
            }

            ImGui.EndChild();
        }

        private void OpenProjectCallback(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || sender is not OpenFileDialog dialog) return;
            ProjectManager.Load(dialog.SelectedFile!);
            Close();
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
            ImGui.BeginChild("Content"u8, avail);

            icon.Image(new(48));
            ImGui.SameLine();
            ImGui.Text("Create a new Project"u8);

            ImGui.Dummy(new(0, 20));

            ImGui.Indent(48);

            ImGui.Text("Project Name:"u8);

            if (ImGui.InputText("##ProjectName"u8, ref newProjectName, 1024))
            {
                newProjectPath = Path.Combine(EditorConfig.Default.ProjectsFolder, newProjectName);
                canCreateProject = IsValidProjectDir(newProjectPath) && !string.IsNullOrWhiteSpace(newProjectName);
                canCreateFailReason = canCreateProject ? null : string.IsNullOrWhiteSpace(newProjectName) ? $"{UwU.Warning} Project name cannot be empty." : $"{UwU.Warning} Project already exists.";
            }

            ImGui.TextDisabled(newProjectPath);

            if (!canCreateProject)
            {
                ImGui.Dummy(new(0, 20));
                ImGui.TextColored(new(1, 0, 0, 1), canCreateFailReason ?? string.Empty);
            }

            ImGui.Unindent();

            ImGui.EndChild();

            ImGui.BeginTable("#Table"u8, 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn(""u8, ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(""u8);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);

            if (ImGui.Button("Cancel"u8))
            {
                CreateProjectDialogReset();
            }
            ImGui.SameLine();

            ImGui.BeginDisabled(!canCreateProject);

            if (ImGui.Button("Create"u8))
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

        private unsafe void DisplayEntry(HistoryEntry entry, Icon icon, Vector2 padding, Vector2 spacing, float lineHeight, Vector2 entrySize)
        {
            byte* buffer = stackalloc byte[512];
            StrBuilder builder = new(buffer, 512);
            Vector2 pos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new(entrySize.X, pos.Y + padding.Y));

            if (!entry.Pinned)
            {
                ImGuiManager.PushFont("Icons-Regular", 16);
            }
            if (ImGui.SmallButton(builder.BuildLabelId(UwU.Bookmark, entry.Path)))
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

            ImGuiManager.PopFont();

            ImGui.SetCursorPos(pos);

            if (ImGui.Button(builder.BuildId(entry.Path), entrySize))
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

            builder.Reset();
            builder.Append(entry.LastAccess, "dd/MM/yyyy HH:mm");
            builder.End();

            float size = ImGui.CalcTextSize(builder).X;

            ImGui.SetCursorPos(new(entrySize.X - size, nextPos.Y));
            ImGui.Text(builder);

            nextPos.Y += spacing.Y + lineHeight;
            nextPos.X += 5;

            ImGui.SetCursorPos(nextPos);

            ImGui.TextDisabled(entry.Path);

            ImGui.SetCursorPosY(pos.Y + entrySize.Y + spacing.Y);
        }

        private static unsafe void DisplayEntryContextMenu(HistoryEntry entry)
        {
            byte* buffer = stackalloc byte[512];
            StrBuilder builder = new(buffer, 512);
            if (ImGui.BeginPopupContextItem(builder.BuildId(entry.Path)))
            {
                if (ImGui.MenuItem(builder.BuildLabel(UwU.Trash, " Remove from List"u8)))
                {
                    ProjectHistory.RemoveEntryByPath(entry.Path);
                }
                if (!entry.Pinned)
                {
                    ImGuiManager.PushFont("Icons-Regular", 16);
                }
                if (!entry.Pinned && ImGui.MenuItem(builder.BuildLabel(UwU.Bookmark, " Pin"u8)))
                {
                    ProjectHistory.Pin(entry.Path);
                }
                if (entry.Pinned && ImGui.MenuItem(builder.BuildLabel(UwU.Bookmark, " Unpin")))
                {
                    ProjectHistory.Unpin(entry.Path);
                }
                ImGuiManager.PopFont();
                if (ImGui.MenuItem(builder.BuildLabel(UwU.Copy, " Copy Path")))
                {
                    Clipboard.SetText(entry.Path);
                }

                ImGui.EndPopup();
            }
        }
    }
}