namespace HexaEngine.Core.Materials.Nodes.Intrinsics
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class SqrtNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public SqrtNode(int id, bool removable, bool isStatic) : base(id, "Sqrt", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public SqrtNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "sqrt";
    }
}