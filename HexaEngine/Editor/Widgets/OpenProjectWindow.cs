namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
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

            base.Draw();
        }

        protected override unsafe void DrawContent()
        {
            ImGui.SetWindowPos(new(0, 0));
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
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
                            ProjectManager.Load(r);
                            Close();
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
                    ProjectManager.Load(entry.Path);
                    Close();
                }
            }
        }

        public override void Reset()
        {
        }
    }
}