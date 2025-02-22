namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class FracNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public FracNode(int id, bool removable, bool isStatic) : base(id, "Frac", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public FracNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "frac";
    }
}