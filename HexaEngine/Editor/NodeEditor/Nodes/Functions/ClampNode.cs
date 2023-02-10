namespace HexaEngine.Editor.NodeEditor.Nodes.Functions
{
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class ClampNode : MathFuncBaseNode
    {
        public ClampNode(int id, bool removable, bool isStatic) : base(id, "Clamp", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            InValue = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Value", PinShape.QuadFilled, PinKind.Input, mode, 1));
            InMin = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Min", PinShape.QuadFilled, PinKind.Input, mode, 1));
            InMax = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Max", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public Pin InValue;
        public Pin InMin;
        public Pin InMax;

        public override string Op { get; } = "clamp";
    }
}