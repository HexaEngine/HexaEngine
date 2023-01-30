namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;

    public class FloatConstantNode : Node
    {
        public FloatConstantNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Float", removable, isStatic)
        {
            CreatePin("out", PinKind.Output, PinType.Float, ImNodesNET.PinShape.CircleFilled);
        }

        public float Value;

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            ImGui.InputFloat("Value", ref Value);
            ImGui.PopItemWidth();
        }
    }
}