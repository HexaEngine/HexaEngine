namespace HexaEngine.Editor.Materials.Nodes.Functions
{
    using HexaEngine.Editor.Materials.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class MinNode : MathFuncBaseNode
    {
        public MinNode(int id, bool removable, bool isStatic) : base(id, "Min", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, mode, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "min";
    }
}