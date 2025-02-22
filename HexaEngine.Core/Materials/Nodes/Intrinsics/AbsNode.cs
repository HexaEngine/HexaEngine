namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class AbsNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public AbsNode(int id, bool removable, bool isStatic) : base(id, "Abs", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
            AddAllowedMode(PinType.AnyInt);
        }

        public AbsNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "abs";
    }
}