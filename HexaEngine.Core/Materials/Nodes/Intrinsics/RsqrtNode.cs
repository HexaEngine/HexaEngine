namespace HexaEngine.Core.Materials.Nodes.Intrinsics
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class RsqrtNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public RsqrtNode(int id, bool removable, bool isStatic) : base(id, "Rsqrt", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public RsqrtNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "rsqrt";
    }
}