namespace HexaEngine.Editor.Dialogs
{
    using ImGuiNET;

    public class DeleteFileDialog : Modal
    {
        private string fileName = string.Empty;

        public override string Name { get; } = "Warning Delete File";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

        public string FileName { get => fileName; set => fileName = value; }

        protected override void DrawContent()
        {
            ImGui.Text($"Are you sure ");
            ImGui.Text(fileName);

            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Ok"))
            {
                File.Delete(fileName);
                Close();
            }
        }

        public override void Reset()
        {
        }
    }

    public class RenameFileDialog
    {
        private bool shown;
        private string file = string.Empty;
        private string filename = string.Empty;
        private string dirname = string.Empty;
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
                dirname = Path.GetDirectoryName(file) ?? string.Empty;
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
            if (!shown)
            {
                return false;
            }

            bool result = false;
            if (ImGui.Begin("Rename file", ref shown, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFocus();

                ImGui.Text(dirname);
                ImGui.SameLine();
                ImGui.InputText("##New name", ref filename, 2048);

                if (ImGui.Button("Cancel"))
                {
                    renameFileResult = RenameFileResult.Cancel;
                    result = true;
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

    public class RenameDirectoryDialog
    {
        private bool shown;
        private string dir = string.Empty;
        private string dirName = string.Empty;
        private string dirParentName = string.Empty;
        private RenameFileResult renameFileResult;
        private bool overwrite;

        public RenameDirectoryDialog()
        {
        }

        public RenameDirectoryDialog(string file)
        {
            Dir = file;
        }

        public RenameDirectoryDialog(bool overwrite)
        {
            Overwrite = overwrite;
        }

        public RenameDirectoryDialog(string dir, bool overwrite)
        {
            Dir = dir;
            Overwrite = overwrite;
        }

        public bool Shown => shown;

        public string Dir
        {
            get => dir; set
            {
                dir = value;
                dirName = Path.GetFileName(dir);
                dirParentName = Path.GetDirectoryName(dir) ?? string.Empty;
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
            if (!shown)
            {
                return false;
            }

            bool result = false;
            if (ImGui.Begin("Rename folder", ref shown, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFocus();

                ImGui.Text(dirParentName);
                ImGui.SameLine();
                ImGui.InputText("##New name", ref dirName, 2048);

                if (ImGui.Button("Cancel"))
                {
                    renameFileResult = RenameFileResult.Cancel;
                    result = true;
                }
                ImGui.SameLine();
                if (ImGui.Button("Ok"))
                {
                    string dir = new(Path.GetDirectoryName(this.dir.AsSpan()));
                    string newPath = Path.Combine(dir, dirName);
                    Directory.Move(this.dir, newPath);
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