namespace HexaEngine.Editor.MaterialEditor.Nodes.Buildin
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using Hexa.NET.ImNodes;

    public class LitNode : FuncCallNodeBase
    {
        public LitNode(int id, bool removable, bool isStatic) : base(id, "Lit", removable, isStatic)
        {
            lockType = true;
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "NdotL", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "NdotH", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "m", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            base.Initialize(editor);
            Out.Type = PinType.Float4;
            UpdateMode();
        }

        public override string Op { get; } = "lit";
    }
}