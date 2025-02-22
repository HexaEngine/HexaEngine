namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class SwizzleVectorNode : InferTypedNodeBase
    {
        private string mask = "xyzw";

        [JsonConstructor]
        public SwizzleVectorNode(int id, bool removable, bool isStatic) : base(id, "Swizzle Vector", removable, isStatic)
        {
            Mode = PinType.AnyFloat;
            LockOutputType = true;
        }

        public SwizzleVectorNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            In = CreateOrGetPin(editor, "in", PinKind.Input, PinType.DontCare, PinShape.CircleFilled, 1);
            Out = CreateOrGetPin(editor, "out", PinKind.Output, PinType.Float4, PinShape.CircleFilled);
            base.Initialize(editor);
            UpdateMode();
        }

        [JsonIgnore]
        public Pin In { get; private set; } = null!;

        [JsonIgnore]
        public Pin Out { get; private set; } = null!;

        [JsonIgnore]
        public override PinType[] Modes => PinTypeHelper.NumericAnyTypes;

        [JsonIgnore]
        public override string ModesComboString => PinTypeHelper.NumericAnyTypesCombo;

        public string Mask
        {
            get => mask; set
            {
                mask = value;
                UpdateMask();
            }
        }

        public override void UpdateMode()
        {
            base.UpdateMode();
            UpdateMask();
        }

        public virtual void UpdateMask()
        {
            if ((In?.Links.Count ?? 0) == 0) return;
            var scalar = Mode.ToScalar();

            if (mask.Length == 0 || mask.Length > 4) return;

            var vector = scalar + (mask.Length - 1);
            Out.Type = vector;
            Type = vector.ToSType();
        }
    }
}