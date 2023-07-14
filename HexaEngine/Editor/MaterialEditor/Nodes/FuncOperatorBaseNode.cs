namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;
    using Newtonsoft.Json;

    public abstract class FuncOperatorBaseNode : TypedNodeBase, IFuncOperatorNode
    {
        protected FuncOperatorBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
        }

        [JsonIgnore]
        public FloatPin InLeft { get; private set; }

        [JsonIgnore]
        public FloatPin InRight { get; private set; }

        [JsonIgnore]
        public abstract string Op { get; }

        [JsonIgnore]
        public Pin Out { get; private set; }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", ImNodesPinShape.QuadFilled, PinKind.Output, mode));
            InLeft = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            InRight = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        protected override void UpdateMode()
        {
            if (lockType)
                return;
            InLeft.Type = mode;
            InRight.Type = mode;
            base.UpdateMode();
        }
    }
}