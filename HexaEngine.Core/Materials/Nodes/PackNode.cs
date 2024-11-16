namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class PackNode : Node, ITypedNode
    {
        [JsonIgnore] public PinType mode = PinType.Float4;
        [JsonIgnore] public string[] names;
        [JsonIgnore] public PinType[] modes;
        [JsonIgnore] public int[] componentMasks = [1, 2, 3, 4];
        [JsonIgnore] public int item = 3;

        [JsonIgnore]
        public FloatPin[] InPins = new FloatPin[4];

        public PackNode(int id, bool removable, bool isStatic) : base(id, "Pack Vector", removable, isStatic)
        {
            TitleColor = 0x5D3874FF.RGBAToVec4();
            TitleHoveredColor = 0x6F4C85FF.RGBAToVec4();
            TitleSelectedColor = 0x79578FFF.RGBAToVec4();
            modes = [PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4];
            names = modes.Select(x => x.ToString()).ToArray();
        }

        public PinType Mode { get => mode; set => mode = value; }

        public int Item { get => item; set => item = value; }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, mode));
            InPins[0] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "x", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            InPins[1] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "y", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            InPins[2] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "z", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            InPins[3] = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "w", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public void UpdateMode()
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
                    Type = new(ScalarType.Float);
                    break;

                case PinType.Float2:
                    Type = new(VectorType.Float2);
                    break;

                case PinType.Float3:
                    Type = new(VectorType.Float3);
                    break;

                case PinType.Float4:
                    Type = new(VectorType.Float4);
                    break;
            }
        }

        [JsonIgnore]
        public SType Type { get; private set; }

        [JsonIgnore]
        public Pin Out { get; private set; } = null!;
    }
}