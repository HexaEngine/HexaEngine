namespace HexaEngine.Editor.MaterialEditor.Nodes.Buildin
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using Hexa.NET.ImNodes;

    public class RefractNode : FuncCallNodeBase
    {
        public RefractNode(int id, bool removable, bool isStatic) : base(id, "Refract", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "refract";
    }
}