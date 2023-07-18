namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using ImGuiNET;
    using ImNodesNET;
    using Newtonsoft.Json;

    public class SplitNode : Node, ITypedNode
    {
        private PinType mode = PinType.Float4;
        private string[] names;
        private PinType[] modes;
        private int[] componentMasks = { 1, 2, 3, 4 };
        private int item = 3;

        [JsonIgnore]
        public FloatPin[] OutPins = new FloatPin[4];

#pragma warning disable CS8618 // Non-nullable field 'In' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public SplitNode(int id, bool removable, bool isStatic) : base(id, "Vector to Components", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable field 'In' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
            modes = new PinType[] { PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4 };
            names = modes.Select(x => x.ToString()).ToArray();
        }

        public PinType Mode { get => mode; set => mode = value; }

        public int Item { get => item; set => item = value; }

        public override void Initialize(NodeEditor editor)
        {
            In = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "in", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            OutPins[0] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "x", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float));
            OutPins[1] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "y", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float));
            OutPins[2] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "z", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float));
            OutPins[3] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "w", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float));
            base.Initialize(editor);
            UpdateMode();
        }

        private void UpdateMode()
        {
            for (int i = 0; i < 4; i++)
            {
                DestroyPin(OutPins[i]);
            }
            for (int i = 0; i < componentMasks[item]; i++)
            {
                AddPin(OutPins[i]);
            }
            In.Type = mode;
            SType type = mode switch
            {
                PinType.Float => new(Generator.Enums.ScalarType.Float),
                PinType.Float2 => new(Generator.Enums.VectorType.Float2),
                PinType.Float3 => new(Generator.Enums.VectorType.Float3),
                PinType.Float4 => new(Generator.Enums.VectorType.Float4),
                _ => throw new NotImplementedException(),
            };
            Type = new(type.Name);
        }

        [JsonIgnore]
        public SType Type { get; private set; }

        [JsonIgnore]
        public Pin In;

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