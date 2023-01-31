namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.Materials.Generator;
    using ImGuiNET;

    public abstract class MathBaseNode : Node, IMathOpNode
    {
        private PinType mode = PinType.Float;
        private string[] names;
        private PinType[] modes;
        private int item;

        protected MathBaseNode(NodeEditor graph, string name, bool removable, bool isStatic) : base(graph, name, removable, isStatic)
        {
            modes = new PinType[] { PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4 };
            names = modes.Select(x => x.ToString()).ToArray();
            
            Out = CreatePin("out", PinKind.Output, mode, ImNodesNET.PinShape.QuadFilled);
            InLeft = CreatePin("Left", PinKind.Input, mode, ImNodesNET.PinShape.QuadFilled, 1);
            InRight = CreatePin("Right", PinKind.Input, mode, ImNodesNET.PinShape.QuadFilled, 1);
            UpdateMode();
        }


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

                case PinType.Float2:
                    Type = new(Materials.Generator.Enums.VectorType.Float2);
                    break;

                case PinType.Float3:
                    Type = new(Materials.Generator.Enums.VectorType.Float3);
                    break;

                case PinType.Float4:
                    Type = new(Materials.Generator.Enums.VectorType.Float4);
                    break;
            }
        }

        protected override void DrawContentBeforePins()
        {
            ImGui.PushItemWidth(100);
            if (ImGui.Combo("##Mode", ref item, names, names.Length))
            {
                mode = modes[item];
                UpdateMode();
            }
            ImGui.PopItemWidth();
        }

        public SType Type { get; protected set; }

        public Pin InLeft { get; }

        public Pin InRight { get; }

        public abstract string Op { get; }

        public Pin Out { get; }
    }
}