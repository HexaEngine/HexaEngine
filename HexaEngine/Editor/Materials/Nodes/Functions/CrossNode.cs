namespace HexaEngine.Editor.Materials.Nodes.Functions
{
    using HexaEngine.Editor.Materials.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class CrossNode : MathFuncBaseNode
    {
        public CrossNode(int id, bool removable, bool isStatic) : base(id, "Cross", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            InLeft = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, mode, 1));
            InRight = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public Pin InLeft;
        public Pin InRight;

        public override string Op { get; } = "cross";
    }
}