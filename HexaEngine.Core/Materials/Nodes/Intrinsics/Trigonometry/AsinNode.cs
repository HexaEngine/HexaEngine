namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Trigonometry
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class AsinNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public AsinNode(int id, bool removable, bool isStatic) : base(id, "asin", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public AsinNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "asin";
    }
}