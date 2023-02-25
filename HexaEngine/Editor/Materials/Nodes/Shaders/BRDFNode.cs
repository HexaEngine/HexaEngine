namespace HexaEngine.Editor.Materials.Nodes.Shaders
{
    using HexaEngine.Editor.Materials.Generator;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class BRDFNode : MethodNode
    {
#pragma warning disable CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        public BRDFNode(int id, bool removable, bool isStatic) : base(id, "BRDF", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, PinType.Float3));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "BaseColor", PinShape.QuadFilled, PinKind.Input, PinType.Float4, new(0.8f, 0.8f, 0.8f, 1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Roughness", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Metallic", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Specular", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Specular Tint", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Sheen", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Sheen Tint", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Clearcoat", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Clearcoat Gloss", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.03f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Anisotropic", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Subsurface", PinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Tangent", PinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            base.Initialize(editor);
        }

        public override FloatPin Out { get; protected set; }

        [JsonIgnore]
        public override string MethodName => "BRDF";

        [JsonIgnore]
        public override SType Type { get; } = new SType(Generator.Enums.VectorType.Float3);

        public override void DefineMethod(VariableTable table)
        {
            table.AddInclude("brdf.hlsl");
        }
    }
}