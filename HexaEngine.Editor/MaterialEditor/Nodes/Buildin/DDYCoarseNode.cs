namespace HexaEngine.Editor.MaterialEditor.Nodes.Buildin
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using Hexa.NET.ImNodes;

    public class DDYCoarseNode : FuncCallNodeBase
    {
        public DDYCoarseNode(int id, bool removable, bool isStatic) : base(id, "ddy corase", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Value", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "ddy_coarse";
    }
}