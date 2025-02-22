namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class AsFloatNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public AsFloatNode(int id, bool removable, bool isStatic) : base(id, "As Float", removable, isStatic)
        {
        }

        public AsFloatNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "asfloat";

        public override void UpdateMode()
        {
            base.UpdateMode();
            OverwriteMode(PinType.Float + (Mode.ComponentCount() - 1));
        }
    }
}