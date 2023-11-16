namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Reflection;

    public class StringPropertyEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;
        private readonly EditorPropertyMode mode;
        private readonly string relativeTo;
        private readonly OpenFileDialog dialog;

        public StringPropertyEditor(EditorPropertyAttribute nameAttr, PropertyInfo property)
        {
            Name = nameAttr.Name;
            Property = property;
            guiName = new(nameAttr.Name);

            mode = nameAttr.Mode;
            relativeTo = nameAttr.RelativeTo;
            if (mode == EditorPropertyMode.Filepicker)
            {
                dialog = new(nameAttr.StartingPath ?? Paths.CurrentProjectFolder, nameAttr.Filter);
            }
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            string? val = (string?)value;
            if (val == null)
            {
                return false;
            }
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(0);
            switch (mode)
            {
                case EditorPropertyMode.Default:
                    if (ImGui.InputText(guiName.Id, ref val, 2048))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    break;

                case EditorPropertyMode.Filepicker:
                    if (ImGui.InputText(guiName.Id, ref val, 2048, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        value = val;
                        ImGui.PopItemWidth();
                        return true;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"...##{guiName.RawId}"))
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
                            ImGui.PopItemWidth();
                            return true;
                        }
                    }
                    break;
            }
            ImGui.PopItemWidth();
            return false;
        }
    }
}