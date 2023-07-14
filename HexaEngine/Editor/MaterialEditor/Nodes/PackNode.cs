namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using ImGuiNET;
    using ImNodesNET;
    using Newtonsoft.Json;

    public class PackNode : Node, ITypedNode
    {
        private PinType mode = PinType.Float4;
        private string[] names;
        private PinType[] modes;
        private int[] componentMasks = { 1, 2, 3, 4 };
        private int item = 3;

        [JsonIgnore]
        public FloatPin[] InPins = new FloatPin[4];

        public PackNode(int id, bool removable, bool isStatic) : base(id, "Components to Vector", removable, isStatic)
        {
            modes = new PinType[] { PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4 };
            names = modes.Select(x => x.ToString()).ToArray();
        }

        public PinType Mode { get => mode; set => mode = value; }

        public int Item { get => item; set => item = value; }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", ImNodesPinShape.QuadFilled, PinKind.Output, mode));
            InPins[0] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "x", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            InPins[1] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "y", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            InPins[2] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "z", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            InPins[3] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "w", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        private void UpdateMode()
        {
            for (int i = 0; i < 4; i++)
            {
                DestroyPin(InPins[i]);
            }
            for (int i = 0; i < componentMasks[item]; i++)
            {
                AddPin(InPins[i]);
            }
            Out.Type = mode;
            switch (mode)
            {
                case PinType.Float:
                    Type = new(Generator.Enums.ScalarType.Float);
                    break;

                case PinType.Float2:
                    Type = new(Generator.Enums.VectorType.Float2);
                    break;

                case PinType.Float3:
                    Type = new(Generator.Enums.VectorType.Float3);
                    break;

                case PinType.Float4:
                    Type = new(Generator.Enums.VectorType.Float4);
                    break;
            }
        }

        [JsonIgnore]
        public SType Type { get; private set; }

        [JsonIgnore]
        public Pin Out;

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
    }
}