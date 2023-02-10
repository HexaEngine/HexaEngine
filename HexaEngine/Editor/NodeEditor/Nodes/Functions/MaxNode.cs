namespace HexaEngine.Editor.NodeEditor.Nodes.Functions
{
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class MaxNode : MathFuncBaseNode
    {
        public MaxNode(int id, bool removable, bool isStatic) : base(id, "Max", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, mode, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "max";
    }
}