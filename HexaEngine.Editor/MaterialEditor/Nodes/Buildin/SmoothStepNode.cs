namespace HexaEngine.Editor.MaterialEditor.Nodes.Buildin
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using Hexa.NET.ImNodes;

    public class SmoothStepNode : FuncCallNodeBase
    {
        private FloatPin mix;

        public SmoothStepNode(int id, bool removable, bool isStatic) : base(id, "SmoothStep", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            mix = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "V", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "smoothstep";

        protected override void UpdateMode()
        {
            base.UpdateMode();
            mix.Type = PinType.Float;
        }
    }
}