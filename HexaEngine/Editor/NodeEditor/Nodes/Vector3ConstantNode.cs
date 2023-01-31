namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using System.Numerics;

    public class Vector3ConstantNode : Node
    {
        public Vector3ConstantNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Vector3", removable, isStatic)
        {
            CreatePin("out", PinKind.Output, PinType.Float3, ImNodesNET.PinShape.CircleFilled);
        }

        public Vector3 Value;

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            ImGui.InputFloat3("Value", ref Value);
            ImGui.PopItemWidth();
        }
    }
}