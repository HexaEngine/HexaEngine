namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class DistanceNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public DistanceNode(int id, bool removable, bool isStatic) : base(id, "Distance", removable, isStatic)
        {
            LockOutputType = true;
        }

        public DistanceNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "distance";
    }
}