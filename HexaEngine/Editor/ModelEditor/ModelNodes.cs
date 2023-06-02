namespace HexaEngine.Editor.ModelEditor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Editor.MeshEditor;
    using HexaEngine.ImGuiNET;
    using System.Collections.Generic;

    public class ModelNodes : EditorWindow
    {
        private readonly ModelEditorWindow editor;
        private Node? selected;
        private readonly List<Node> openNodes = new();

        public ModelNodes(ModelEditorWindow editor)
        {
            this.editor = editor;
            IsShown = true;
        }

        protected override string Name => "Model Nodes";

        public Node? Selected => selected;

        public override void DrawContent(IGraphicsContext context)
        {
            if (editor.Current == null)
            {
                selected = null;
                return;
            }

            var model = editor.Current;
            var root = model.Root;
            DisplayNode(root);
        }

        private void DisplayNode(Node node)
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (selected == node)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }

            if (node.Children.Count == 0)
            {
                flags |= ImGuiTreeNodeFlags.Leaf;
            }

            bool isOpen = ImGui.TreeNodeEx(node.Name, flags);
            if (isOpen)
            {
                if (!openNodes.Contains(node))
                    openNodes.Add(node);
            }
            else
            {
                openNodes.Remove(node);
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                selected = node;
            }

            if (isOpen)
            {
                for (int j = 0; j < node.Children.Count; j++)
                {
                    DisplayNode(node.Children[j]);
                }
                ImGui.TreePop();
            }
            else
            {
                for (int j = 0; j < node.Children.Count; j++)
                {
                    openNodes.Remove(node.Children[j]);
                }
            }
        }
    }
}