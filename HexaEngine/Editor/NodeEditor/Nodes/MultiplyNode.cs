namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.Materials.Generator;
    using ImGuiNET;

    public class MultiplyNode : Node, IMathOpNode
    {
        public MultiplyNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Add", removable, isStatic)
        {
            Out = CreatePin("out", PinKind.Output, mode, ImNodesNET.PinShape.QuadFilled);
            InLeft = CreatePin("Left", PinKind.Input, mode, ImNodesNET.PinShape.QuadFilled, 1);
            InRight = CreatePin("Right", PinKind.Input, mode, ImNodesNET.PinShape.QuadFilled, 1);
            UpdateMode();
        }

        public Pin Out { get; }
        public Pin InLeft { get; }
        public Pin InRight { get; }
        public string Op { get; } = "*";
        public SType Type { get; private set; }

        private PinType mode = PinType.Vector4;

        private void UpdateMode()
        {
            Out.Type = mode;
            InLeft.Type = mode;
            InRight.Type = mode;
            switch (mode)
            {
                case PinType.Float:
                    Type = new(Materials.Generator.Enums.ScalarType.Float);
                    break;

                case PinType.Vector2:
                    Type = new(Materials.Generator.Enums.VectorType.Float2);
                    break;

                case PinType.Vector3:
                    Type = new(Materials.Generator.Enums.VectorType.Float3);
                    break;

                case PinType.Vector4:
                    Type = new(Materials.Generator.Enums.VectorType.Float4);
                    break;
            }
        }

        protected override void DrawContent()
        {
            base.DrawContent();
            if (ImGui.RadioButton("Float", mode == PinType.Float))
            {
                mode = PinType.Float;
                UpdateMode();
            }
            if (ImGui.RadioButton("Float2", mode == PinType.Vector2))
            {
                mode = PinType.Vector2;
                UpdateMode();
            }
            if (ImGui.RadioButton("Float3", mode == PinType.Vector3))
            {
                mode = PinType.Vector3;
                UpdateMode();
            }
            if (ImGui.RadioButton("Float4", mode == PinType.Vector4))
            {
                mode = PinType.Vector4;
                UpdateMode();
            }
        }
    }
}