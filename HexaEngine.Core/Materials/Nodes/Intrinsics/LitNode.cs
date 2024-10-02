namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class LitNode : FuncCallNodeBase
    {
        public LitNode(int id, bool removable, bool isStatic) : base(id, "Lit", removable, isStatic)
        {
            lockType = true;
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "NdotL", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "NdotH", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "m", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1));
            base.Initialize(editor);
            Out.Type = PinType.Float4;
            UpdateMode();
        }

        public override string Op { get; } = "lit";
    }
}