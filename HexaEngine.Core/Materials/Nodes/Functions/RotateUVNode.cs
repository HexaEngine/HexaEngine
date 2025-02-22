namespace HexaEngine.Core.Materials.Nodes.Functions
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public enum UVRotationMode
    {
        None,
        Midpoint,
    }

    public class RotateUVNode : FuncCallDeclarationBaseNode
    {
        [JsonConstructor]
        public RotateUVNode(int id, bool removable, bool isStatic) : base(id, "Rotate UV", removable, isStatic)
        {
        }

        public RotateUVNode() : this(0, true, false)
        {
        }

        [JsonIgnore]
        public override string MethodName { get; } = "RotateUV";

        [JsonIgnore]
        public override SType Type { get; } = new SType(ScalarType.Float);

        public override PrimitivePin Out { get; protected set; } = null!;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.CircleFilled, PinKind.Output, PinType.Float2));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "in", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "rotation", PinShape.CircleFilled, PinKind.Input, PinType.Float, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "mid", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1));

            base.Initialize(editor);
        }

        public override void DefineMethod(GenerationContext context, VariableTable table)
        {
            string body = @"
	return float2(
      cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
      cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
    );";
            table.AddMethod("RotateUV", "float2 uv, float rotation, float2 mid", "float2", body);
        }
    }
}