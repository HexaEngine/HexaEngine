namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class PowNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public PowNode(int id, bool removable, bool isStatic) : base(id, "Pow", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public PowNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, Mode, 1, PinFlags.InferType));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, Mode, 1, PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "pow";
    }
}