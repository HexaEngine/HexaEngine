namespace HexaEngine.Editor.MaterialEditor.Nodes.Functions
{
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class DotNode : MathFuncBaseNode
    {
        public DotNode(int id, bool removable, bool isStatic) : base(id, "Dot", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "dot";

        protected override void UpdateMode()
        {
            base.UpdateMode();
            Out.Type = PinType.Float;
        }
    }
}