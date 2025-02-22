namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class AsUIntNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public AsUIntNode(int id, bool removable, bool isStatic) : base(id, "As UInt", removable, isStatic)
        {
        }

        public AsUIntNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            base.Initialize(editor);
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "asuint";

        public override void UpdateMode()
        {
            base.UpdateMode();
            OverwriteMode(PinType.UInt + (Mode.ComponentCount() - 1));
        }
    }
}