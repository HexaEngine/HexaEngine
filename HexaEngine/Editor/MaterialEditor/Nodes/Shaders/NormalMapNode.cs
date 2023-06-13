namespace HexaEngine.Editor.MaterialEditor.Nodes.Shaders
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.MaterialEditor.Generator.Enums;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class NormalMapNode : MethodNode
    {
#pragma warning disable CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        public NormalMapNode(int id, bool removable, bool isStatic) : base(id, "Normal Map", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float3));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Tangent", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Bitangent", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal Tex", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));

            base.Initialize(editor);
        }

        public override FloatPin Out { get; protected set; }

        [JsonIgnore]
        public override string MethodName => "BRDF";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float3);

        public override void DefineMethod(VariableTable table)
        {
            table.AddInclude("brdf.hlsl");
        }
    }
}