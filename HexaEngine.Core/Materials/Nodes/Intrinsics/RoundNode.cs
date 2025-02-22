namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class RoundNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public RoundNode(int id, bool removable, bool isStatic) : base(id, "Round", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public RoundNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "round";
    }
}