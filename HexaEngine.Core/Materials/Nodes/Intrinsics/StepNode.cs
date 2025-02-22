namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class StepNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public StepNode(int id, bool removable, bool isStatic) : base(id, "Step", removable, isStatic)
        {
        }

        public StepNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "y", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "x", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "step";
    }
}