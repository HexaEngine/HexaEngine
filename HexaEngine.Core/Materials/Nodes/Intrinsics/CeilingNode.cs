namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class CeilingNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public CeilingNode(int id, bool removable, bool isStatic) : base(id, "Ceiling", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public CeilingNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "ceil";
    }
}