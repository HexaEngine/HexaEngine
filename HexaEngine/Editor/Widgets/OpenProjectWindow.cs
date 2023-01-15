namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Projects;
    using ImGuiNET;
    using System;

    public class OpenProjectWindow : ImGuiWindow
    {
        private static bool filePickerIsOpen = false;
        private static bool fileSaverIsOpen = false;
        private static readonly FilePicker filePicker = new(Environment.CurrentDirectory);
        private static readonly FileSaver fileSaver = new(Environment.CurrentDirectory);
        private static Action<FilePickerResult, string>? filePickerCallback;
        private static Action<FilePickerResult, FileSaver>? fileSaverCallback;

        public OpenProjectWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.Modal | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration;
        }

        protected override string Name => "Open Project";

        public override void DrawWindow(IGraphicsContext context)
        {
            if (filePickerIsOpen)
            {
                if (filePicker.Draw())
                {
                    filePickerCallback?.Invoke(filePicker.Result, filePicker.SelectedFile);
                }
            }

            if (fileSaverIsOpen)
            {
                if (fileSaver.Draw())
                {
                    fileSaverCallback?.Invoke(fileSaver.Result, fileSaver);
                }
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
                    filePickerIsOpen = true;
                    filePicker.AllowedExtensions.Add(".hexproj");
                    filePicker.OnlyAllowFilteredExtensions = true;
                    filePickerCallback = (e, r) =>
                    {
                        if (e == FilePickerResult.Ok)
                        {
                            ProjectManager.Load(r);
                            IsShown = false;
                        }
                        filePickerIsOpen = false;
                        filePicker.AllowedExtensions.Clear();
                        filePicker.OnlyAllowFilteredExtensions = false;
                    };
                }

                if (ImGui.MenuItem("New"))
                {
                    fileSaverIsOpen = true;
                    fileSaverCallback = (e, r) =>
                    {
                        if (e == FilePickerResult.Ok)
                        {
                            Directory.CreateDirectory(Path.Combine(r.CurrentFolder, r.SelectedFile));
                            ProjectManager.Create(Path.Combine(r.CurrentFolder, r.SelectedFile));
                            IsShown = false;
                        }
                        fileSaverIsOpen = false;
                    };
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