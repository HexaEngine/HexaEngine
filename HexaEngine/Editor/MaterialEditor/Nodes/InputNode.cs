namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.ImNodesNET;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;

    public class InputNode : Node
    {
        public InputNode(int id, bool removable, bool isStatic) : base(id, "Geometry", removable, isStatic)
        {
            TitleColor = MathUtil.PackARGB(0xff, 0x23, 0x00, 0xc8);
            TitleHoveredColor = MathUtil.PackARGB(0xff, 0x28, 0x00, 0xe4);
            TitleSelectedColor = MathUtil.PackARGB(0xff, 0x2d, 0x00, 0xff);
        }

        public override void Initialize(NodeEditor editor)
        {
            WorldPos = CreateOrGetPin(editor, "pos", PinKind.Output, PinType.Float3, ImNodesPinShape.CircleFilled);
            TexCoord = CreateOrGetPin(editor, "uv", PinKind.Output, PinType.Float2, ImNodesPinShape.CircleFilled);
            Normal = CreateOrGetPin(editor, "normal", PinKind.Output, PinType.Float3, ImNodesPinShape.CircleFilled);
            Tangent = CreateOrGetPin(editor, "tangent", PinKind.Output, PinType.Float3, ImNodesPinShape.CircleFilled);
            Bitangent = CreateOrGetPin(editor, "binormal", PinKind.Output, PinType.Float3, ImNodesPinShape.CircleFilled);
            base.Initialize(editor);
        }

        [JsonIgnore]
        public Pin WorldPos;

        [JsonIgnore]
        public Pin TexCoord;

        [JsonIgnore]
        public Pin Normal;

        [JsonIgnore]
        public Pin Tangent;

        [JsonIgnore]
        public Pin Bitangent;
    }
}