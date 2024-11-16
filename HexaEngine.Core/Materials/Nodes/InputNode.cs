namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes.Textures;
    using Newtonsoft.Json;

    public class InputNode : Node
    {
        public InputNode(int id, bool removable, bool isStatic) : base(id, "Geometry", removable, isStatic)
        {
            TitleColor = 0xC82A2AFF.RGBAToVec4();
            TitleHoveredColor = 0xDA3C3CFF.RGBAToVec4();
            TitleSelectedColor = 0xE04949FF.RGBAToVec4();
        }

        public override void Initialize(NodeEditor editor)
        {
            Color = CreateOrGetPin(editor, "color", PinKind.Output, PinType.Float4, PinShape.CircleFilled);
            WorldPos = CreateOrGetPin(editor, "pos", PinKind.Output, PinType.Float3, PinShape.CircleFilled);
            TexCoord = CreateOrGetPin(editor, "uv", PinKind.Output, PinType.Float2, PinShape.CircleFilled);
            Normal = CreateOrGetPin(editor, "normal", PinKind.Output, PinType.Float3, PinShape.CircleFilled);
            Tangent = CreateOrGetPin(editor, "tangent", PinKind.Output, PinType.Float3, PinShape.CircleFilled);
            Bitangent = CreateOrGetPin(editor, "binormal", PinKind.Output, PinType.Float3, PinShape.CircleFilled);
            ViewDir = CreateOrGetPin(editor, "viewDir", PinKind.Output, PinType.Float3, PinShape.CircleFilled);
            base.Initialize(editor);
        }

        [JsonIgnore]
        public Pin Color { get; private set; } = null!;

        [JsonIgnore]
        public Pin WorldPos { get; private set; } = null!;

        [JsonIgnore]
        public Pin TexCoord { get; private set; } = null!;

        [JsonIgnore]
        public Pin Normal { get; private set; } = null!;

        [JsonIgnore]
        public Pin Tangent { get; private set; } = null!;

        [JsonIgnore]
        public Pin Bitangent { get; private set; } = null!;

        [JsonIgnore]
        public Pin ViewDir { get; private set; } = null!;
    }
}