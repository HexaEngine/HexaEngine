namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Conditional
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class AllNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public AllNode(int id, bool removable, bool isStatic) : base(id, "all", removable, isStatic)
        {
        }

        public AllNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "all";
    }
}