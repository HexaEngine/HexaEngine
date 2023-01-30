namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using System.Numerics;

    public class Vector4ConstantNode : Node
    {
        public Vector4ConstantNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Vector4", removable, isStatic)
        {
            CreatePin("out", PinKind.Output, PinType.Vector4, ImNodesNET.PinShape.CircleFilled);
        }

        public Vector4 Value;

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            ImGui.InputFloat4("Value", ref Value);
            ImGui.PopItemWidth();
        }
    }
}