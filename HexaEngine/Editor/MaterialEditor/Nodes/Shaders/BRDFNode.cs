namespace HexaEngine.Editor.MaterialEditor.Nodes.Shaders
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.MaterialEditor.Generator.Enums;
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
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float4));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "BaseColor", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, new(0.8f, 0.8f, 0.8f, 1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Roughness", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Metallic", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Reflectance", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "AO", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Emissive", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, PinFlags.ColorEdit));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Emissive Strength", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            base.Initialize(editor);
        }

        public override FloatPin Out { get; protected set; }

        [JsonIgnore]
        public override string MethodName => "BRDF";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float3);

        public override void DefineMethod(VariableTable table)
        {
        }
    }

    public class BRDFClothNode : MethodNode
    {
#pragma warning disable CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        public BRDFClothNode(int id, bool removable, bool isStatic) : base(id, "BRDF Cloth", removable, isStatic)
#pragma warning restore CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", ImNodesPinShape.QuadFilled, PinKind.Output, PinType.Float4));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "BaseColor", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, new(0.8f, 0.8f, 0.8f, 1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Normal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Roughness", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "AO", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Clear Coat", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Clear Coat Roughness", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Anisotropy", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Anisotropy Direction", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Sheen Color", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, new(1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Emissive", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, PinFlags.ColorEdit));
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Emissive Strength", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            base.Initialize(editor);
        }

        public override FloatPin Out { get; protected set; }

        [JsonIgnore]
        public override string MethodName => "BRDF";

        [JsonIgnore]
        public override SType Type { get; } = new SType(VectorType.Float3);

        public override void DefineMethod(VariableTable table)
        {
        }
    }
}