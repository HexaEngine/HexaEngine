namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Projects;
    using ImGuiNET;
    using System;

    public class OpenProjectWindow : ImGuiWindow
    {
        private static readonly OpenFileDialog filePicker = new(Environment.CurrentDirectory);
        private static readonly SaveFileDialog fileSaver = new(Environment.CurrentDirectory);
        private static Action<OpenFileResult, string>? filePickerCallback;
        private static Action<SaveFileResult, SaveFileDialog>? fileSaverCallback;

        public OpenProjectWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.Modal | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking;
        }

        protected override string Name => "Open Project";

        public override void DrawWindow(IGraphicsContext context)
        {
            if (filePicker.Draw())
            {
                filePickerCallback?.Invoke(filePicker.Result, filePicker.SelectedFile);
            }

            if (fileSaver.Draw())
            {
                fileSaverCallback?.Invoke(fileSaver.Result, fileSaver);
            }

            base.DrawWindow(context);
        }

        public override void DrawContent(IGraphicsContext context)
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
                            IsShown = false;
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
                            Directory.CreateDirectory(Path.Combine(r.CurrentFolder, r.SelectedFile));
                            ProjectManager.Create(Path.Combine(r.CurrentFolder, r.SelectedFile));
                            IsShown = false;
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
                    IsShown = false;
                }
            }
        }
    }
}