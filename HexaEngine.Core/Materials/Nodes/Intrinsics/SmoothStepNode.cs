namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class SmoothStepNode : FuncCallNodeBase
    {
        private FloatPin mix = null!;

        public SmoothStepNode(int id, bool removable, bool isStatic) : base(id, "SmoothStep", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, mode, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, mode, 1));
            mix = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "V", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "smoothstep";

        public override void UpdateMode()
        {
            base.UpdateMode();
            mix.Type = PinType.Float;
        }
    }
}