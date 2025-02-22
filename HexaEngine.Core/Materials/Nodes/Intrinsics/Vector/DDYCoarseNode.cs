namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Vector
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class DDYCoarseNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public DDYCoarseNode(int id, bool removable, bool isStatic) : base(id, "ddy corase", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public DDYCoarseNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "ddy_coarse";
    }
}