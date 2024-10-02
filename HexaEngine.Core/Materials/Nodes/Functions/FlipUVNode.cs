namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials.Nodes.Textures;
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
            TitleColor = 0x473874FF.RGBAToVec4();
            TitleHoveredColor = 0x685797FF.RGBAToVec4();
            TitleSelectedColor = 0x74679AFF.RGBAToVec4();
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