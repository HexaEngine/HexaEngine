namespace HexaEngine.Core.Materials.Nodes.Noise
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class GenericNoiseNode : FuncCallDeclarationBaseNode
    {
        [JsonConstructor]
        public GenericNoiseNode(int id, bool removable, bool isStatic) : base(id, "Generic Noise", removable, isStatic)
        {
        }

        public GenericNoiseNode() : this(0, true, false)
        {
        }

        public override string MethodName { get; } = "Noise";

        public override SType Type { get; } = new SType(ScalarType.Float);

        public override PrimitivePin Out { get; protected set; } = null!;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.CircleFilled, PinKind.Output, PinType.Float));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "in", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "scale", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1));

            base.Initialize(editor);
        }

        public override void DefineMethod(GenerationContext context, VariableTable table)
        {
            string body = @"
	return frac(sin(dot(uv * scale, float2(12.9898, 4.1414))) * 43758.5453);";
            table.AddMethod("Noise", "float2 uv, float2 scale", "float", body);
        }
    }
}