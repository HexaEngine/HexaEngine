namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class AsDoubleNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public AsDoubleNode(int id, bool removable, bool isStatic) : base(id, "As Double", removable, isStatic)
        {
        }

        public AsDoubleNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "asdouble";

        public override void UpdateMode()
        {
            base.UpdateMode();
            OverwriteMode(PinType.Double + (Mode.ComponentCount() - 1));
        }
    }
}