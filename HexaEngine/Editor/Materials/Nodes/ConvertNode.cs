namespace HexaEngine.Editor.Materials.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using ImGuiNET;
    using ImNodesNET;

    public class ConvertNode : Node
    {
#pragma warning disable CS8618 // Non-nullable field 'Out' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'In' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public ConvertNode(int id, bool removable, bool isStatic) : base(id, "Convert", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable field 'In' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'Out' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            In = CreateOrGetPin(editor, "in", PinKind.Input, PinType.Float3, PinShape.CircleFilled);
            Out = CreateOrGetPin(editor, "out", PinKind.Output, PinType.Float4, PinShape.CircleFilled);
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