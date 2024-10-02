namespace HexaEngine.Core.Materials.Nodes.Noise
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class GenericNoiseNode : FuncCallDeclarationBaseNode
    {
        private FloatPin InUV;
        private FloatPin Scale;

        public GenericNoiseNode(int id, bool removable, bool isStatic) : base(id, "Generic Noise", removable, isStatic)
        {
        }

        public override string MethodName { get; } = "Noise";

        public override SType Type { get; } = new SType(ScalarType.Float);

        public override FloatPin Out { get; protected set; }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.CircleFilled, PinKind.Output, PinType.Float));
            InUV = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "in", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv"));
            Scale = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "scale", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1));

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