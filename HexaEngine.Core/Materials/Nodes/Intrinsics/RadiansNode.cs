namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class RadiansNode : FuncCallNodeBase
    {
        public RadiansNode(int id, bool removable, bool isStatic) : base(id, "Radians", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "radians";
    }
}