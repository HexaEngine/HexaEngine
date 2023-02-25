namespace HexaEngine.Editor.Materials.Nodes.Functions
{
    using HexaEngine.Editor.Materials.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class ClampNode : MathFuncBaseNode
    {
#pragma warning disable CS8618 // Non-nullable field 'InMax' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'InMin' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'InValue' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        public ClampNode(int id, bool removable, bool isStatic) : base(id, "Clamp", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable field 'InValue' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'InMin' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'InMax' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
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