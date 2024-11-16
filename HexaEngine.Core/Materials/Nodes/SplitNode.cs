namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class SplitNode : Node, ITypedNode
    {
        [JsonIgnore] private static readonly int[] componentMasks = [1, 2, 3, 4];
        [JsonIgnore] public PinType mode = PinType.Float4;
        [JsonIgnore] public int item = 3;

        [JsonIgnore]
        public FloatPin[] OutPins = new FloatPin[4];

        public SplitNode(int id, bool removable, bool isStatic) : base(id, "Split Vector", removable, isStatic)
        {
            TitleColor = 0x5D3874FF.RGBAToVec4();
            TitleHoveredColor = 0x6F4C85FF.RGBAToVec4();
            TitleSelectedColor = 0x79578FFF.RGBAToVec4();
        }

        public PinType Mode { get => mode; set => mode = value; }

        public int Item { get => item; set => item = value; }

        public override void Initialize(NodeEditor editor)
        {
            In = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, mode, 1));
            OutPins[0] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "x", PinShape.QuadFilled, PinKind.Output, PinType.Float));
            OutPins[1] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "y", PinShape.QuadFilled, PinKind.Output, PinType.Float));
            OutPins[2] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "z", PinShape.QuadFilled, PinKind.Output, PinType.Float));
            OutPins[3] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "w", PinShape.QuadFilled, PinKind.Output, PinType.Float));
            base.Initialize(editor);
            UpdateMode();
        }

        public void UpdateMode()
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
                PinType.Float => new(ScalarType.Float),
                PinType.Float2 => new(VectorType.Float2),
                PinType.Float3 => new(VectorType.Float3),
                PinType.Float4 => new(VectorType.Float4),
                _ => throw new NotImplementedException(),
            };
            Type = new(type.Name);
        }

        [JsonIgnore]
        public SType Type { get; private set; }

        [JsonIgnore]
        public Pin In { get; private set; } = null!;
    }
}