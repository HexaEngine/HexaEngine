namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using System.Numerics;

    public class Color4ConstantNode : Node
    {
        public Color4ConstantNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Color4", removable, isStatic)
        {
            CreatePin("out", PinKind.Output, PinType.Vector4, ImNodesNET.PinShape.CircleFilled);
        }

        public Vector4 Value;

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            ImGui.ColorEdit4("Value", ref Value);
            ImGui.PopItemWidth();
        }
    }
}