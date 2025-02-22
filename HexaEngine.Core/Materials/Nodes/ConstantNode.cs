﻿namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class ConstantNode : TypedNodeBase
    {
        [JsonConstructor]
        public ConstantNode(int id, bool removable, bool isStatic) : base(id, "Constant", removable, isStatic)
        {
            TitleColor = 0x5A37A1FF.RGBAToVec4();
            TitleHoveredColor = 0x6A48B0FF.RGBAToVec4();
            TitleSelectedColor = 0x7D5CBFFF.RGBAToVec4();
            Mode = PinType.Float4;
            Type = Mode.ToSType();
        }

        public ConstantNode() : this(0, true, false)
        {
        }

        [JsonIgnore]
        public override string ModesComboString => PinTypeHelper.NumericTypesCombo;

        [JsonIgnore]
        public override PinType[] Modes => PinTypeHelper.NumericTypes;

        [JsonIgnore]
        public Pin Out { get; private set; } = null!;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Const", PinShape.QuadFilled, PinKind.Output, Mode, uint.MaxValue, PinFlags.AllowOutput));
            base.Initialize(editor);
            UpdateMode();
        }

        public override void UpdateMode()
        {
            Out.Type = Mode;
            base.UpdateMode();
        }
    }
}