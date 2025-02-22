namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class Exp2Node : FuncCallNodeBase
    {
        [JsonConstructor]
        public Exp2Node(int id, bool removable, bool isStatic) : base(id, "Exp2", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public Exp2Node() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "exp2";
    }
}