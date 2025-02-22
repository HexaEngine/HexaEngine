namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Misc
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class LitNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public LitNode(int id, bool removable, bool isStatic) : base(id, "Lit", removable, isStatic)
        {
            LockType = true;
        }

        public LitNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "NdotL", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "NdotH", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "m", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "lit";

        public override void UpdateMode()
        {
            base.UpdateMode();
            OverwriteMode(PinType.Float4);
        }
    }
}