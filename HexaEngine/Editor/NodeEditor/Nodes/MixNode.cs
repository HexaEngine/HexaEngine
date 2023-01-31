﻿namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.Materials.Generator;
    using ImGuiNET;

    public class MixNode : Node
    {
        private PinType mode = PinType.Float4;

        public MixNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Mix", removable, isStatic)
        {
            Out = CreatePin("out", PinKind.Output, mode, ImNodesNET.PinShape.QuadFilled);
            InLeft = CreatePin("Left", PinKind.Input, mode, ImNodesNET.PinShape.QuadFilled, 1);
            InRight = CreatePin("Right", PinKind.Input, mode, ImNodesNET.PinShape.QuadFilled, 1);
            InMix = CreatePin("Mix", PinKind.Input, PinType.Float, ImNodesNET.PinShape.QuadFilled, 1);
            UpdateMode();
        }

        public Pin Out;
        public Pin InLeft;
        public Pin InRight;
        public Pin InMix;
        public SType Type;

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

        protected override void DrawContent()
        {
            base.DrawContent();
            if (ImGui.RadioButton("Float", mode == PinType.Float))
            {
                mode = PinType.Float;
                UpdateMode();
            }
            if (ImGui.RadioButton("Float2", mode == PinType.Float2))
            {
                mode = PinType.Float2;
                UpdateMode();
            }
            if (ImGui.RadioButton("Float3", mode == PinType.Float3))
            {
                mode = PinType.Float3;
                UpdateMode();
            }
            if (ImGui.RadioButton("Float4", mode == PinType.Float4))
            {
                mode = PinType.Float4;
                UpdateMode();
            }
        }
    }
}