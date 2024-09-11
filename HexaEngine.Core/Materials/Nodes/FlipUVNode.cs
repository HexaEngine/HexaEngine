namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Pins;

    public enum FlipMode
    {
        None = 0,
        U,
        V,
        Both
    }

    public class FlipUVNode : Node
    {
        public FlipUVNode(int id, bool removable, bool isStatic) : base(id, "Flip UV", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            InUV = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "in", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv"));
            Out = CreateOrGetPin(editor, "out", PinKind.Output, PinType.Float2, PinShape.CircleFilled);
            base.Initialize(editor);
        }

        [JsonIgnore]
        public Pin InUV;

        [JsonIgnore]
        public Pin Out;

        public FlipMode FlipMode;
    }
}