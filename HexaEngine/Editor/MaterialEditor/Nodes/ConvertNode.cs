namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using ImNodesNET;
    using Newtonsoft.Json;

    public class ConvertNode : Node
    {
        public ConvertNode(int id, bool removable, bool isStatic) : base(id, "Convert", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            In = CreateOrGetPin(editor, "in", PinKind.Input, PinType.Float3, ImNodesPinShape.CircleFilled);
            Out = CreateOrGetPin(editor, "out", PinKind.Output, PinType.Float4, ImNodesPinShape.CircleFilled);
            base.Initialize(editor);
        }

        [JsonIgnore]
        public Pin In;

        [JsonIgnore]
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