namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Mathematics;
    using ImNodesNET;
    using Newtonsoft.Json;

    public class BRDFShadingModelNode : Node, ITypedNode
    {
        public BRDFShadingModelNode(int id, bool removable, bool isStatic) : base(id, "BRDF", removable, isStatic)
        {
            TitleColor = MathUtil.Pack(0xff, 0x0f, 0x9e, 0x00);
            TitleHoveredColor = MathUtil.Pack(0xff, 0x13, 0xc4, 0x00);
            TitleSelectedColor = MathUtil.Pack(0xff, 0x16, 0xe4, 0x00);
        }

        [JsonIgnore]
        public SType Type { get; }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "BaseColor", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, new(1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.normal"));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Roughness", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Metallic", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Reflectance", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "AO", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Emissive", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, 1, PinFlags.ColorEdit));
            base.Initialize(editor);
        }
    }
}