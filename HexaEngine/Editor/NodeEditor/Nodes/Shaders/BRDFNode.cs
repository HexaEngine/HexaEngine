namespace HexaEngine.Editor.NodeEditor.Nodes.Shaders
{
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImNodesNET;

    public class BRDFNode : BaseMethodNode
    {
        public BRDFNode(int id, bool removable, bool isStatic) : base(id, "BRDF", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, PinType.Float4));
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
        public override string MethodName => "";

        public override string GetMethod()
        {
            throw new NotImplementedException();
        }
    }
}