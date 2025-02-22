namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class PackNode : TypedNodeBase
    {
        [JsonConstructor]
        public PackNode(int id, bool removable, bool isStatic) : base(id, "Pack Vector", removable, isStatic)
        {
            TitleColor = 0x5D3874FF.RGBAToVec4();
            TitleHoveredColor = 0x6F4C85FF.RGBAToVec4();
            TitleSelectedColor = 0x79578FFF.RGBAToVec4();
        }

        public PackNode() : this(0, true, false)
        {
        }

        [JsonIgnore]
        public Pin Out { get; private set; } = null!;

        [JsonIgnore]
        public UniversalPin[] InPins { get; } = new UniversalPin[4];

        [JsonIgnore]
        public override string ModesComboString => PinTypeHelper.NumericTypesCombo;

        [JsonIgnore]
        public override PinType[] Modes => PinTypeHelper.NumericTypes;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, Mode));
            var scalarType = Mode.ToScalar();
            InPins[0] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "x", PinShape.QuadFilled, PinKind.Input, scalarType, 1));
            InPins[1] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "y", PinShape.QuadFilled, PinKind.Input, scalarType, 1));
            InPins[2] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "z", PinShape.QuadFilled, PinKind.Input, scalarType, 1));
            InPins[3] = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "w", PinShape.QuadFilled, PinKind.Input, scalarType, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override void UpdateMode()
        {
            base.UpdateMode();

            for (int i = 0; i < 4; i++)
            {
                DestroyPin(InPins[i]);
            }

            int componentCount = Mode.ComponentCount();
            var scalarType = Mode.ToScalar();
            for (int i = 0; i < componentCount; i++)
            {
                var pin = InPins[i];
                pin.Type = scalarType;
                AddPin(pin);
            }

            Out.Type = Mode;
            Type = Mode.ToSType();
        }
    }
}