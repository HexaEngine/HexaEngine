namespace HexaEngine.Materials.Nodes.Functions
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class NormalMapNode : FuncCallDeclarationBaseNode
    {
        [JsonConstructor]
        public NormalMapNode(int id, bool removable, bool isStatic) : base(id, "Normal Map", removable, isStatic)
        {
        }

        public NormalMapNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, PinType.Float3));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "sample", PinShape.QuadFilled, PinKind.Input, PinType.Float4, 1, defaultExpression: "0"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.normal"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Tangent", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.tangent"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Binormal", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.binormal"));

            base.Initialize(editor);
        }

        [JsonIgnore]
        public override PrimitivePin Out { get; protected set; } = null!;

        [JsonIgnore]
        public override string MethodName => "NormalMap";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float3);

        public override void DefineMethod(GenerationContext context, VariableTable table)
        {
            string body = @"
	// Uncompress each component from [0,1] to [-1,1].
    float3 normalT = 2.0f * normalMapSample.xyz - 1.0f;

	// Build orthonormal basis.
    float3 N = unitNormalW;
    float3 T = normalize(tangentW - dot(tangentW, N) * N);
    float3 B = cross(N, T);

    float3x3 TBN = float3x3(T, B, N);

	// Transform from tangent space to world space.
    float3 bumpedNormalW = mul(normalT, TBN);

    return bumpedNormalW;";
            table.AddMethod("NormalMap", "float4 normalMapSample, float3 unitNormalW, float3 tangentW, float3 bitangent", "float3", body);
        }
    }
}