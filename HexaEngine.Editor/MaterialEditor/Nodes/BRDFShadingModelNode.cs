namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImNodes;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.NodeEditor;
    using Hexa.NET.Mathematics;
    using Newtonsoft.Json;

    public class BRDFShadingModelNode : Node, ITypedNode
    {
        public BRDFShadingModelNode(int id, bool removable, bool isStatic) : base(id, "BRDF", removable, isStatic)
        {
            TitleColor = MathUtil.PackARGB(0xff, 0x0f, 0x9e, 0x00);
            TitleHoveredColor = MathUtil.PackARGB(0xff, 0x13, 0xc4, 0x00);
            TitleSelectedColor = MathUtil.PackARGB(0xff, 0x16, 0xe4, 0x00);
        }

        [JsonIgnore]
        public SType Type { get; }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "BaseColor", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, new(1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Normal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.normal"));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Roughness", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Metallic", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Reflectance", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "AO", "AmbientOcclusion", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Emissive", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, 1, PinFlags.ColorEdit));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Displacement", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1, PinFlags.None));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Displacement Factor", "DisplacementFactor", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, 1, PinFlags.None));
            base.Initialize(editor);
        }
    }
}