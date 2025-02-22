namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Vector
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class DDYNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public DDYNode(int id, bool removable, bool isStatic) : base(id, "ddy", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public DDYNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "ddy";
    }
}