namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.Dialogs;
    using ImGuiNET;

    public class StringPropertyEditor : IPropertyEditor
    {
        private readonly string id;
        private readonly string name;
        private readonly EditorPropertyMode mode;
        private readonly string relativeTo;
        private readonly OpenFileDialog dialog;

        public StringPropertyEditor(string name, EditorPropertyMode mode, string? startingPath, string? filter, string? relativeTo)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
            this.mode = mode;
            this.relativeTo = relativeTo;
            if (mode == EditorPropertyMode.Filepicker)
            {
                dialog = new(startingPath ?? Paths.CurrentProjectFolder, filter);
            }
        }

        public bool Draw(object instance, ref object? value)
        {
            string? val = (string?)value;
            if (val == null)
            {
                return false;
            }

            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputText($"{name}##{id}", ref val, 2048))
                    {
                        value = val;
                        return true;
                    }
                    break;

                case EditorPropertyMode.Filepicker:
                    if (ImGui.InputText($"{name}##{id}", ref val, 2048, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        value = val;
                        return true;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"...##{id}"))
                    {
                        dialog.Show();
                    }
                    if (dialog.Draw())
                    {
                        if (dialog.Result == OpenFileResult.Ok)
                        {
                            if (relativeTo != null)
                            {
                                value = Path.GetRelativePath(relativeTo, FileSystem.GetRelativePath(dialog.FullPath));
                            }
                            else
                            {
                                value = FileSystem.GetRelativePath(dialog.FullPath);
                            }

                            return true;
                        }
                    }
                    break;
            }

            return false;
        }
    }
}