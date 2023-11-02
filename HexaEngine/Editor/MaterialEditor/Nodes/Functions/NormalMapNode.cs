namespace HexaEngine.Editor.MaterialEditor.Nodes.Functions
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.MaterialEditor.Generator.Enums;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;

    public class NormalMapNode : FuncCallDeclarationBaseNode
    {
#pragma warning disable CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        public NormalMapNode(int id, bool removable, bool isStatic) : base(id, "Normal Map", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float3));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "sample", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "0"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.normal"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Tangent", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.tangent"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Binormal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.binormal"));

            base.Initialize(editor);
        }

        public override FloatPin Out { get; protected set; }

        [JsonIgnore]
        public override string MethodName => "NormalMap";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float3);

        public override void DefineMethod(VariableTable table)
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