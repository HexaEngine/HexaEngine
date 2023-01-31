namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using System.Numerics;

    public class Vector2ConstantNode : Node
    {
        public Vector2ConstantNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Vector2", removable, isStatic)
        {
            CreatePin("out", PinKind.Output, PinType.Float2, ImNodesNET.PinShape.CircleFilled);
        }

        public Vector2 Value;

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            ImGui.InputFloat2("Value", ref Value);
            ImGui.PopItemWidth();
        }
    }
}