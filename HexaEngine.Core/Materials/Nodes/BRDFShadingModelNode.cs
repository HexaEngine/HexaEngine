namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes.Textures;
    using Newtonsoft.Json;

    public class BRDFShadingModelNode : Node, ITypedNode
    {
        public BRDFShadingModelNode(int id, bool removable, bool isStatic) : base(id, "BRDF", removable, isStatic)
        {
            TitleColor = 0x31762CFF.RGBAToVec4();
            TitleHoveredColor = 0x4D9648FF.RGBAToVec4();
            TitleSelectedColor = 0x6FB269FF.RGBAToVec4();
        }

        [JsonIgnore]
        public SType Type { get; }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "BaseColor", PinShape.QuadFilled, PinKind.Input, PinType.Float4, new(1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Normal", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.normal"));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Roughness", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Metallic", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Reflectance", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "AO", "AmbientOcclusion", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Emissive", PinShape.QuadFilled, PinKind.Input, PinType.Float4, 1, PinFlags.ColorEdit));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Displacement", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1, PinFlags.None));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Displacement Factor", "DisplacementFactor", PinShape.QuadFilled, PinKind.Input, PinType.Float, 1, PinFlags.None));
            base.Initialize(editor);
        }
    }
}