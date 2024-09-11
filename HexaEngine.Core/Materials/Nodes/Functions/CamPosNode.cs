namespace HexaEngine.Materials.Nodes.Functions
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class CamPosNode : FuncCallDeclarationBaseNode
    {
#pragma warning disable CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        public CamPosNode(int id, bool removable, bool isStatic) : base(id, "camera Pos", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, PinType.Float3));

            base.Initialize(editor);
        }

        public override void DefineMethod(VariableTable table)
        {
            table.AddInclude("cam.hlsl");
        }

        public override FloatPin Out { get; protected set; }

        [JsonIgnore]
        public override string MethodName => "GetCameraPos";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float3);
    }
}