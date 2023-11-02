﻿namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Projects;
    using Hexa.NET.ImGui;

    public class ProjectExplorer : EditorWindow
    {
        private struct Item
        {
            public string Path;
            public string Name;

            public Item(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }

        public ProjectExplorer()
        {
            IsShown = false;
        }

        protected override string Name => "Project";

        private static void DisplayNodeContextMenu(HexaItem element)
        {
            ImGui.PushID(element.Name);
            if (ImGui.BeginPopupContextItem(element.Name))
            {
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        private static void HandleDragDrop(HexaItem item)
        {
            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload(nameof(HexaItem));
                    if (!payload.IsNull)
                    {
                        string path = *(UnsafeOldString*)payload.Data;
                        // TODO: ObjectAdded global drag drop handler
                    }
                }
                ImGui.EndDragDropTarget();
            }

            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    var str = new UnsafeOldString(item.GetAbsolutePath());
                    ImGui.SetDragDropPayload(nameof(HexaItem), &str, (uint)sizeof(nint));
                }
                ImGui.Text(item.Name);
                ImGui.EndDragDropSource();
            }
        }

        private void DisplayNode(HexaItem element)
        {
            if (element is HexaDirectory directory)
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
                if (element.IsSelected)
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }

                if (directory.Items.Count == 0)
                {
                    flags |= ImGuiTreeNodeFlags.Leaf;
                }

                bool isOpen = ImGui.TreeNodeEx(element.Name, flags);
                directory.IsExpanded = isOpen;
                directory.IsVisible = true;

                DisplayNodeContextMenu(element);

                HandleDragDrop(element);

                if (isOpen)
                {
                    for (int j = 0; j < directory.Items.Count; j++)
                    {
                        DisplayNode(directory.Items[j]);
                    }
                    ImGui.TreePop();
                }
                else
                {
                    for (int j = 0; j < directory.Items.Count; j++)
                    {
                        directory.Items[j].IsVisible = false;
                    }
                }
            }
            else
            {
                ImGui.TreeNodeEx(element.Name, ImGuiTreeNodeFlags.Leaf);

                DisplayNodeContextMenu(element);

                HandleDragDrop(element);

                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    Designer.OpenFile(element.GetAbsolutePath());
                }
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            HexaProject? project = ProjectManager.Project;
            if (project == null)
            {
                return;
            }

            for (int i = 0; i < project.Items.Count; i++)
            {
                DisplayNode(project.Items[i]);
            }
        }
    }
}