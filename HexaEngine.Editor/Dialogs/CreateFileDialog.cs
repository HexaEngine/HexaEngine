namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.ImGui.Widgets.Dialogs;

    public class CreateFileDialog : Dialog
    {
        private string basePath;
        private string fileName = "";
        private string fullPath = null!;
        private bool allowOverwrite;
        private string? extension;

        public CreateFileDialog(bool allowOverwrite = false)
        {
            basePath = "";
            this.allowOverwrite = allowOverwrite;
        }

        public CreateFileDialog(string basePath, bool allowOverwrite = false)
        {
            this.basePath = basePath;
            this.allowOverwrite = allowOverwrite;
        }

        public override string Name { get; } = "Create File";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.AlwaysAutoResize;

        public string BasePath { get => basePath; set => basePath = value; }

        public string FileName { get => fileName; set => fileName = value; }

        public string FullPath { get => fullPath; set => fullPath = value; }

        public string? Extension { get => extension; set => extension = value; }

        public bool AllowOverwrite { get => allowOverwrite; set => allowOverwrite = value; }

        protected override void DrawContent()
        {
            if (ImGui.InputText("Name", ref fileName, 255))
            {
            }

            if (ImGui.Button("Cancel"))
            {
                Close(DialogResult.Cancel);
            }

            ImGui.SameLine();

            if (ImGui.Button("Create"))
            {
                Close(DialogResult.Ok);
            }
        }

        public override void Close()
        {
            if (Validate())
            {
                base.Close();
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            fullPath = Path.Combine(basePath, $"{fileName}{extension}");
            if (File.Exists(fullPath))
            {
                if (allowOverwrite)
                {
                    MessageBox.Show("Overwrite file?", $"Do you want to overwrite '{fullPath}'", this, OverwriteDialogCallback, MessageBoxType.YesNo);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void OverwriteDialogCallback(MessageBox sender, object? args)
        {
            if (sender.Result == MessageBoxResult.Yes)
            {
                Result = DialogResult.Ok;
                base.Close();
            }
        }
    }
}