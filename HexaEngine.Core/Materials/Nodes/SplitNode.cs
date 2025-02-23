namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class SplitNode : InferTypedNodeBase, INodeDropConnector
    {
        [JsonConstructor]
        public SplitNode(int id, bool removable, bool isStatic) : base(id, "Split Vector", removable, isStatic)
        {
            TitleColor = 0x5D3874FF.RGBAToVec4();
            TitleHoveredColor = 0x6F4C85FF.RGBAToVec4();
            TitleSelectedColor = 0x79578FFF.RGBAToVec4();
        }

        public SplitNode() : this(0, true, false)
        {
        }

        [JsonIgnore]
        public Pin In { get; private set; } = null!;

        [JsonIgnore]
        public UniversalPin[] OutPins { get; } = new UniversalPin[4];

        [JsonIgnore]
        public override string ModesComboString => PinTypeHelper.NumericTypesCombo;

        [JsonIgnore]
        public override PinType[] Modes => PinTypeHelper.NumericTypes;

        public override void Initialize(NodeEditor editor)
        {
            In = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, PinFlags.InferType));
            var scalarType = Mode.ToScalar();
            OutPins[0] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "x", PinShape.QuadFilled, PinKind.Output, scalarType));
            OutPins[1] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "y", PinShape.QuadFilled, PinKind.Output, scalarType));
            OutPins[2] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "z", PinShape.QuadFilled, PinKind.Output, scalarType));
            OutPins[3] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "w", PinShape.QuadFilled, PinKind.Output, scalarType));
            base.Initialize(editor);
            UpdateMode();
        }

        public override void UpdateMode()
        {
            base.UpdateMode();
            for (int i = 0; i < 4; i++)
            {
                DestroyPin(OutPins[i]);
            }

            if (Mode == PinType.Unknown)
            {
                Type = SType.Unknown;
                return;
            }

            int componentCount = Mode.ComponentCount();
            var scalarType = Mode.ToScalar();
            for (int i = 0; i < componentCount; i++)
            {
                var pin = OutPins[i];
                pin.Type = scalarType;
                AddPin(pin);
            }

            SType type = Mode.ToSType();
            Type = new(type.Name);
        }

        void INodeDropConnector.Connect(Pin outputPin)
        {
            editor?.TryCreateLink(In, outputPin);
        }
    }
}