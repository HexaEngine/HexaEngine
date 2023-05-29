namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.ImNodesNET;

    public class InputNode : Node
    {
#pragma warning disable CS8618 // Non-nullable field 'Tangent' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'Normal' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'TexCoord' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'WorldPos' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public InputNode(int id, bool removable, bool isStatic) : base(id, "Geometry", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable field 'WorldPos' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'TexCoord' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'Normal' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning restore CS8618 // Non-nullable field 'Tangent' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
            TitleColor = 0xc80023ff;
            TitleHoveredColor = 0xe40028ff;
            TitleSelectedColor = 0xff002dff;
        }

        public override void Initialize(NodeEditor editor)
        {
            WorldPos = CreateOrGetPin(editor, "pos", PinKind.Output, PinType.Float4, ImNodesPinShape.CircleFilled);
            TexCoord = CreateOrGetPin(editor, "tex", PinKind.Output, PinType.Float2, ImNodesPinShape.CircleFilled);
            Normal = CreateOrGetPin(editor, "normal", PinKind.Output, PinType.Float3, ImNodesPinShape.CircleFilled);
            Tangent = CreateOrGetPin(editor, "tangent", PinKind.Output, PinType.Float3, ImNodesPinShape.CircleFilled);
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
    }
}