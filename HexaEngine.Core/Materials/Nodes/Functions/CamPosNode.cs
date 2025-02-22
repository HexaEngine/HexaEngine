﻿namespace HexaEngine.Materials.Nodes.Functions
{
    using HexaEngine.Core.Materials;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class CamPosNode : FuncCallDeclarationBaseNode
    {
        [JsonConstructor]
        public CamPosNode(int id, bool removable, bool isStatic) : base(id, "Camera Pos", removable, isStatic)
        {
            TitleColor = 0x473874FF.RGBAToVec4();
            TitleHoveredColor = 0x685797FF.RGBAToVec4();
            TitleSelectedColor = 0x74679AFF.RGBAToVec4();
        }

        public CamPosNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, PinType.Float3));

            base.Initialize(editor);
        }

        public override void DefineMethod(GenerationContext context, VariableTable table)
        {
            table.AddInclude("../../camera.hlsl");
        }

        public override PrimitivePin Out { get; protected set; } = null!;

        [JsonIgnore]
        public override string MethodName => "GetCameraPos";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float3);
    }
}