namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Mathematics;
    using ImNodesNET;
    using Newtonsoft.Json;
    using System.Numerics;

    public class PropertyPin : FloatPin
    {
        public PropertyPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks, flags, defaultExpression)
        {
            PropertyName = name;
        }

        public PropertyPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, Vector4 value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, value, maxLinks, flags, defaultExpression)
        {
            PropertyName = name;
        }

        public PropertyPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, float valueX, float valueY, float valueZ, float valueW, string defaultExpression) : base(id, name, shape, kind, type, maxLinks, flags, valueX, valueY, valueZ, valueW, defaultExpression)
        {
            PropertyName = name;
        }

        public PropertyPin(int id, string name, string propertyName, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks, flags, defaultExpression)
        {
            PropertyName = propertyName;
        }

        public PropertyPin(int id, string name, string propertyName, ImNodesPinShape shape, PinKind kind, PinType type, Vector4 value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, value, maxLinks, flags, defaultExpression)
        {
            PropertyName = propertyName;
        }

        [JsonConstructor]
        public PropertyPin(int id, string name, string propertyName, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, float valueX, float valueY, float valueZ, float valueW, string defaultExpression) : base(id, name, shape, kind, type, maxLinks, flags, valueX, valueY, valueZ, valueW, defaultExpression)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        public static PropertyPin? FindPropertyPin(NodeEditor editor, string propertyName)
        {
            for (int i = 0; i < editor.Nodes.Count; i++)
            {
                var node = editor.Nodes[i];
                for (int j = 0; j < node.Pins.Count; j++)
                {
                    var pin = node.Pins[j];
                    if (pin is PropertyPin propertyPin && propertyPin.PropertyName == propertyName)
                    {
                        return propertyPin;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<PropertyPin> FindPropertyPins(NodeEditor editor, string propertyName)
        {
            for (int i = 0; i < editor.Nodes.Count; i++)
            {
                var node = editor.Nodes[i];
                for (int j = 0; j < node.Pins.Count; j++)
                {
                    var pin = node.Pins[j];
                    if (pin is PropertyPin propertyPin && propertyPin.PropertyName == propertyName)
                    {
                        yield return propertyPin;
                    }
                }
            }
        }
    }

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
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "BaseColor", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, new(1), 1, PinFlags.ColorEdit));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Normal", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float3, 1, defaultExpression: "pixel.normal"));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Roughness", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Metallic", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Reflectance", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(0.5f), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "AO", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float, new(1), 1, PinFlags.Slider));
            AddOrGetPin(new PropertyPin(editor.GetUniqueId(), "Emissive", ImNodesPinShape.QuadFilled, PinKind.Input, PinType.Float4, 1, PinFlags.ColorEdit));
            base.Initialize(editor);
        }
    }
}