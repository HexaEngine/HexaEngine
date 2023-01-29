namespace HexaEngine.Editor.Dialogs
{
    using ImGuiNET;

    public class RenameFileDialog
    {
        private bool shown;
        private string file = string.Empty;
        private string filename = string.Empty;
        private RenameFileResult renameFileResult;
        private bool overwrite;

        public RenameFileDialog()
        {
        }

        public RenameFileDialog(string file)
        {
            File = file;
        }

        public RenameFileDialog(bool overwrite)
        {
            Overwrite = overwrite;
        }

        public RenameFileDialog(string file, bool overwrite)
        {
            File = file;
            Overwrite = overwrite;
        }

        public bool Shown => shown;

        public string File
        {
            get => file; set
            {
                file = value;
                filename = Path.GetFileName(file);
            }
        }

        public bool Overwrite { get => overwrite; set => overwrite = value; }

        public RenameFileResult Result => renameFileResult;

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
            bool result = false;
            if (ImGui.Begin("Rename file", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFocus();

                ImGui.InputText("New name", ref filename, 2048);

                if (ImGui.Button("Cancel"))
                {
                    renameFileResult = RenameFileResult.Cancel;
                }
                ImGui.SameLine();
                if (ImGui.Button("Ok"))
                {
                    string dir = new(Path.GetDirectoryName(file.AsSpan()));
                    string newPath = Path.Combine(dir, filename);
                    System.IO.File.Move(file, newPath, overwrite);
                    renameFileResult = RenameFileResult.Ok;
                    result = true;
                }
                ImGui.End();
            }

            if (result)
            {
                shown = false;
            }

            return result;
        }
    }
}