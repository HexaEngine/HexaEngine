namespace HexaEngine.Editor.Materials.Nodes.Functions
{
    using HexaEngine.Editor.Materials.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class MixNode : MathFuncBaseNode
    {
        private FloatPin mix;

        public MixNode(int id, bool removable, bool isStatic) : base(id, "Mix", removable, isStatic)
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

        public override string Op { get; } = "lerp";

        protected override void UpdateMode()
        {
            base.UpdateMode();
            mix.Type = PinType.Float;
        }
    }
}