namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class AsIntNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public AsIntNode(int id, bool removable, bool isStatic) : base(id, "As Int", removable, isStatic)
        {
        }

        public AsIntNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "asint";

        public override void UpdateMode()
        {
            base.UpdateMode();
            OverwriteMode(PinType.Int + (Mode.ComponentCount() - 1));
        }
    }
}