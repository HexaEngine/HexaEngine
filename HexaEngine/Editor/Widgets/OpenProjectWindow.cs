namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Projects;
    using ImGuiNET;
    using System;

    public class OpenProjectWindow : Modal
    {
        private static readonly OpenFileDialog filePicker = new(Environment.CurrentDirectory);
        private static readonly SaveFileDialog fileSaver = new(Environment.CurrentDirectory);
        private static Action<OpenFileResult, string>? filePickerCallback;
        private static Action<SaveFileResult, SaveFileDialog>? fileSaverCallback;
        private HistoryEntry historyEntry;

        public OpenProjectWindow()
        {
        }

        public override string Name => "Open Project";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.MenuBar;

        public override void Draw()
        {
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

            base.Draw();
        }

        protected override unsafe void DrawContent()
        {
            var size = ImGui.GetWindowSize();
            var s = ImGui.GetPlatformIO().Monitors.Data[0].MainSize;

            ImGui.SetWindowPos(s / 2 - size / 2);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Open"))
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

                if (ImGui.MenuItem("New"))
                {
                    fileSaverCallback = (e, r) =>
                    {
                        if (e == SaveFileResult.Ok)
                        {
                            Directory.CreateDirectory(r.FullPath);
                            ProjectManager.Create(r.FullPath);
                            Close();
                        }
                    };
                    fileSaver.Show();
                }

                ImGui.EndMenuBar();
            }

            ImGui.Separator();

            var entries = ProjectHistory.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (ImGui.MenuItem(entry.Fullname))
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
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem($"ObjectRemoved from List##{i}"))
                    {
                        ProjectHistory.RemoveEntryByPath(historyEntry.Path);
                    }
                    ImGui.EndPopup();
                }
            }
        }

        public override void Reset()
        {
        }
    }
}