namespace HexaEngine.Editor.MaterialEditor.Nodes.Functions
{
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class CrossNode : MathFuncBaseNode
    {
#pragma warning disable CS8618 // Non-nullable field 'InLeft' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'InRight' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public CrossNode(int id, bool removable, bool isStatic) : base(id, "Cross", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable field 'InRight' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'InLeft' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            InLeft = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            InRight = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", ImNodesPinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public Pin InLeft;
        public Pin InRight;

        public override string Op { get; } = "cross";
    }
}