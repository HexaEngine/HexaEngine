namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Editor.Packaging;
    using HexaEngine.Editor.Projects;
    using System.Numerics;

    public class PackageManagerWidget : EditorWindow
    {
        private string searchString = string.Empty;

        protected override string Name { get; } = "\xE7B8 Package Manager";

        public override void DrawContent(IGraphicsContext context)
        {
            var project = ProjectManager.CurrentProject;

            if (project == null)
            {
                ImGui.Text("No Project loaded.");
                return;
            }

            Vector2 pos = ImGui.GetCursorPos();
            Vector2 padding = ImGui.GetStyle().CellPadding;
            Vector2 spacing = ImGui.GetStyle().ItemSpacing;
            float lineHeight = ImGui.GetTextLineHeight();

            const float widthSide = 300;
            Vector2 avail = ImGui.GetContentRegionAvail();
            Vector2 entrySize = new(avail.X - widthSide, ImGui.GetTextLineHeight() * 2 + padding.Y * 2 + spacing.Y);
            Vector2 trueEntrySize = entrySize - new Vector2(ImGui.GetStyle().IndentSpacing, 0);

            Icon icon = IconManager.GetIconByName("Logo") ?? throw new();

            Vector2 entryChildSize = new(entrySize.X, avail.Y);
            ImGui.BeginChild("##Entries", entryChildSize);

            ImGui.InputTextWithHint("##SearchBar", "Search ...", ref searchString, 1024);

            if (ImGui.Button("Add Package"))
            {
                var itemGroup = (ItemGroup?)project.Items.FirstOrDefault(x => x is ItemGroup items && items.Any(y => y is PackageReference));
                if (itemGroup == null)
                {
                    itemGroup = new();
                    project.Items.Add(itemGroup);
                }

                itemGroup.Add(new PackageReference("Test Package", "1.0.0.0"));
                ProjectManager.SaveProjectFile();
            }

            for (int i = 0; i < project.Items.Count; i++)
            {
                var projectItem = project.Items[i];
                if (projectItem is not ItemGroup itemGroup)
                {
                    continue;
                }

                for (int j = 0; j < itemGroup.Count; j++)
                {
                    var item = itemGroup[j];
                    if (item is not PackageReference packageReference)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchString) && !packageReference.Include.Contains(searchString))
                    {
                        continue;
                    }

                    DisplayEntry(packageReference, icon, padding, spacing, lineHeight, trueEntrySize);
                }
            }

            ImGui.EndChild();
        }

        private void DisplayEntry(PackageReference entry, Icon icon, Vector2 padding, Vector2 spacing, float lineHeight, Vector2 entrySize)
        {
            Vector2 pos = ImGui.GetCursorPos();

            if (ImGui.Button($"##{entry.Include}", entrySize))
            {
            }

            DisplayEntryContextMenu(entry);

            ImGui.SetCursorPos(pos + padding);

            Vector2 imageSize = new(entrySize.Y - padding.Y * 2);

            icon.Image(imageSize);

            Vector2 nextPos = new(pos.X + padding.X + imageSize.X + spacing.X, pos.Y + padding.Y);

            ImGui.SetCursorPos(nextPos);

            ImGui.Text(entry.Include);

            float size = ImGui.CalcTextSize(entry.Version).X;

            ImGui.SetCursorPos(new(entrySize.X - size, nextPos.Y));
            ImGui.Text(entry.Version);

            nextPos.Y += spacing.Y + lineHeight;
            nextPos.X += 5;

            ImGui.SetCursorPos(nextPos);

            ImGui.TextDisabled("");

            ImGui.SetCursorPosY(pos.Y + entrySize.Y + spacing.Y);
        }

        private void DisplayEntryContextMenu(PackageReference entry)
        {
        }
    }
}