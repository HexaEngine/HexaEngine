namespace HexaEngine.Editor.MaterialEditor.Nodes.Buildin
{
    using Hexa.NET.ImNodes;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;

    public class TanhNode : FuncCallNodeBase
    {
        public TanhNode(int id, bool removable, bool isStatic) : base(id, "Tanh", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "value", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "tanh";
    }
}