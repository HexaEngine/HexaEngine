namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Vector
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class CrossNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public CrossNode(int id, bool removable, bool isStatic) : base(id, "Cross", removable, isStatic)
        {
            LockType = true;
            LockOutputType = true;
        }

        public CrossNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "cross";

        public override void UpdateMode()
        {
            base.UpdateMode();
            OverwriteMode(PinType.Float3);
        }
    }
}