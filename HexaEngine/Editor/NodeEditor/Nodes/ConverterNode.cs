namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using ImGuiNET;

    public class ConverterNode : Node
    {
        public ConverterNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Converter", removable, isStatic)
        {
            In = CreatePin("in", PinKind.Input, PinType.Float3, ImNodesNET.PinShape.CircleFilled);
            Out = CreatePin("out", PinKind.Output, PinType.Float4, ImNodesNET.PinShape.CircleFilled);
        }

        public Pin In;
        public Pin Out;
        public float Value;

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            ImGui.InputFloat("##Value", ref Value);
            ImGui.PopItemWidth();
            
        }
    }
}
