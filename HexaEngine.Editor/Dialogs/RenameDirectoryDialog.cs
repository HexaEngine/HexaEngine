namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;

    [Obsolete("Use Widgets instead.")]
    public class RenameDirectoryDialog
    {
        private bool shown;
        private string dir = string.Empty;
        private string dirName = string.Empty;
        private string dirParentName = string.Empty;
        private RenameResult renameResult;
        private bool overwrite;

        public RenameDirectoryDialog()
        {
        }

        public RenameDirectoryDialog(string file)
        {
            Directory = file;
        }

        public RenameDirectoryDialog(bool overwrite)
        {
            Overwrite = overwrite;
        }

        public RenameDirectoryDialog(string dir, bool overwrite)
        {
            Directory = dir;
            Overwrite = overwrite;
        }

        public bool Shown => shown;

        public string Directory
        {
            get => dir; set
            {
                dir = value;
                dirName = Path.GetFileName(dir);
                dirParentName = $"{Path.GetDirectoryName(dir) ?? string.Empty}{Path.DirectorySeparatorChar}";
            }
        }

        public bool Overwrite { get => overwrite; set => overwrite = value; }

        public RenameResult Result => renameResult;

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
                    renameResult = RenameResult.Cancel;
                    result = true;
                }
                ImGui.SameLine();
                if (ImGui.Button("Ok"))
                {
                    string dir = new(Path.GetDirectoryName(this.dir.AsSpan()));
                    string newPath = Path.Combine(dir, dirName);
                    System.IO.Directory.Move(this.dir, newPath);
                    renameResult = RenameResult.Ok;
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