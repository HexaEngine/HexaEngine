namespace HexaEngine.Editor.MaterialEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using HexaEngine.Core;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes.Textures;
    using System.Numerics;

    public class BaseNodeRenderer : DisposableRefBase, INodeRenderer
    {
        private bool fullEdit = true;

        public virtual void Draw(Node node)
        {
            if (node.wantsSetPosition)
            {
                ImNodes.SetNodeGridSpacePos(node.Id, node.position);
                node.wantsSetPosition = false;
            }

            ImNodes.PushColorStyle(ImNodesCol.TitleBar, node.TitleColor.Vec4ToABGR());
            ImNodes.PushColorStyle(ImNodesCol.TitleBarHovered, node.TitleHoveredColor.Vec4ToABGR());
            ImNodes.PushColorStyle(ImNodesCol.TitleBarSelected, node.TitleSelectedColor.Vec4ToABGR());
            ImNodes.BeginNode(node.Id);
            ImNodes.BeginNodeTitleBar();
            if (node.isEditing)
            {
                string name = node.Name;
                ImGui.PushItemWidth(100);
                if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    node.Name = name;
                    node.isEditing = false;
                }
                ImGui.PopItemWidth();
            }
            else
            {
                ImGui.Text(node.Name);
                ImGui.SameLine();
                if (ImGui.SmallButton($"{UwU.Pen}##{node.Id}"))
                {
                    node.isEditing = true;
                }
                if (ImGui.BeginItemTooltip())
                {
                    ImGui.Text(node.OriginalName);
                    ImGui.EndTooltip();
                }
            }

            ImNodes.EndNodeTitleBar();

            if (fullEdit && node.isEditing)
            {
                ImGui.PushItemWidth(200);
                Vector4 c = node.TitleColor;
                if (ImGui.ColorEdit4("Title Bar", ref c))
                {
                    node.TitleColor = c;
                }
                Vector4 c2 = node.TitleHoveredColor;
                if (ImGui.ColorEdit4("Title Hovered", ref c2))
                {
                    node.TitleHoveredColor = c2;
                }
                Vector4 c3 = node.TitleSelectedColor;
                if (ImGui.ColorEdit4("Title Bar Selected", ref c3))
                {
                    node.TitleSelectedColor = c3;
                }
                ImGui.PopItemWidth();
                ImNodes.EndNode();
                ImNodes.PopColorStyle();
                return;
            }

            DrawContentBeforePins(node);

            for (int i = 0; i < node.Pins.Count; i++)
            {
                node.Pins[i].Draw();
            }

            DrawContent(node);

            ImNodes.EndNode();
            ImNodes.PopColorStyle();

            node.position = ImNodes.GetNodeGridSpacePos(node.Id);
            node.size = ImNodes.GetNodeDimensions(node.Id);
        }

        protected virtual void DrawContentBeforePins(Node node)
        {
        }

        protected virtual void DrawContent(Node node)
        {
        }

        protected override void DisposeCore()
        {
        }
    }

    public class BaseNodeRenderer<T> : DisposableRefBase, INodeRenderer where T : Node
    {
        private bool fullEdit = true;

        public void Draw(Node node)
        {
            if (node is T t)
            {
                Draw(t);
            }
        }

        public virtual void Draw(T node)
        {
            if (node.wantsSetPosition)
            {
                ImNodes.SetNodeGridSpacePos(node.Id, node.position);
                node.wantsSetPosition = false;
            }

            ImNodes.PushColorStyle(ImNodesCol.TitleBar, node.TitleColor.Vec4ToABGR());
            ImNodes.PushColorStyle(ImNodesCol.TitleBarHovered, node.TitleHoveredColor.Vec4ToABGR());
            ImNodes.PushColorStyle(ImNodesCol.TitleBarSelected, node.TitleSelectedColor.Vec4ToABGR());
            ImNodes.BeginNode(node.Id);
            ImNodes.BeginNodeTitleBar();
            if (node.isEditing)
            {
                string name = node.Name;
                ImGui.PushItemWidth(100);
                if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    node.Name = name;
                    node.isEditing = false;
                }
                ImGui.PopItemWidth();
            }
            else
            {
                ImGui.Text(node.Name);
                ImGui.SameLine();
                if (ImGui.SmallButton($"{UwU.Pen}##{node.Id}"))
                {
                    node.isEditing = true;
                }
                if (ImGui.BeginItemTooltip())
                {
                    ImGui.Text(node.OriginalName);
                    ImGui.EndTooltip();
                }
            }

            ImNodes.EndNodeTitleBar();

            if (fullEdit && node.isEditing)
            {
                ImGui.PushItemWidth(200);
                Vector4 c = node.TitleColor;
                if (ImGui.ColorEdit4("Title Bar", ref c))
                {
                    node.TitleColor = c;
                }
                Vector4 c2 = node.TitleHoveredColor;
                if (ImGui.ColorEdit4("Title Hovered", ref c2))
                {
                    node.TitleHoveredColor = c2;
                }
                Vector4 c3 = node.TitleSelectedColor;
                if (ImGui.ColorEdit4("Title Bar Selected", ref c3))
                {
                    node.TitleSelectedColor = c3;
                }
                ImGui.PopItemWidth();
                ImNodes.EndNode();
                ImNodes.PopColorStyle();
                return;
            }

            DrawContentBeforePins(node);

            for (int i = 0; i < node.Pins.Count; i++)
            {
                node.Pins[i].Draw();
            }

            DrawContent(node);

            ImNodes.EndNode();
            ImNodes.PopColorStyle();

            node.position = ImNodes.GetNodeGridSpacePos(node.Id);
            node.size = ImNodes.GetNodeDimensions(node.Id);
        }

        protected virtual void DrawContentBeforePins(T node)
        {
        }

        protected virtual void DrawContent(T node)
        {
        }

        protected override void DisposeCore()
        {
        }
    }

    public class BaseNodeRendererInstanced<T> : BaseNodeRenderer<T>, INodeRendererInstance<T> where T : Node
    {
        private T node = null!;

        public T Node
        {
            get => node; set { node = value; OnSetInstance(node); }
        }

        public virtual void OnSetInstance(T node)
        {
        }
    }

    public class BasePinRenderer : DisposableRefBase, IPinRenderer
    {
        public virtual void Draw(Pin pin)
        {
            if (pin.Kind == PinKind.Input)
            {
                ImNodes.BeginInputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                DrawContent(pin);
                ImNodes.EndInputAttribute();
            }
            if (pin.Kind == PinKind.Output)
            {
                ImNodes.BeginOutputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                DrawContent(pin);
                ImNodes.EndOutputAttribute();
            }
            if (pin.Kind == PinKind.Static)
            {
                ImNodes.BeginStaticAttribute(pin.Id);
                DrawContent(pin);
                ImNodes.EndStaticAttribute();
            }
        }

        protected virtual void DrawContent(Pin pin)
        {
            ImGui.Text(pin.Name);
        }

        protected override void DisposeCore()
        {
        }
    }

    public class BasePinRenderer<T> : DisposableRefBase, IPinRenderer where T : Pin
    {
        public void Draw(Pin pin)
        {
            if (pin is T t)
            {
                Draw(t);
            }
        }

        public virtual void Draw(T pin)
        {
            if (pin.Kind == PinKind.Input)
            {
                ImNodes.BeginInputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                DrawContent(pin);
                ImNodes.EndInputAttribute();
            }
            if (pin.Kind == PinKind.Output)
            {
                ImNodes.BeginOutputAttribute(pin.Id, (ImNodesPinShape)pin.Shape);
                DrawContent(pin);
                ImNodes.EndOutputAttribute();
            }
            if (pin.Kind == PinKind.Static)
            {
                ImNodes.BeginStaticAttribute(pin.Id);
                DrawContent(pin);
                ImNodes.EndStaticAttribute();
            }
        }

        protected virtual void DrawContent(T pin)
        {
            ImGui.Text(pin.Name);
        }

        protected override void DisposeCore()
        {
        }
    }
}