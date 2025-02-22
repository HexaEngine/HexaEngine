namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Trigonometry
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class RadiansNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public RadiansNode(int id, bool removable, bool isStatic) : base(id, "Radians", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public RadiansNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "radians";
    }
}