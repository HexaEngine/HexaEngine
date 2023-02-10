namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public abstract class MathOpBaseNode : MathBaseNode, IMathOpNode
    {
        protected MathOpBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            InLeft = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, mode, 1));
            InRight = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        [JsonIgnore]
        public FloatPin InLeft { get; private set; }

        [JsonIgnore]
        public FloatPin InRight { get; private set; }

        protected override void UpdateMode()
        {
            InLeft.Type = mode;
            InRight.Type = mode;
            base.UpdateMode();
        }
    }
}