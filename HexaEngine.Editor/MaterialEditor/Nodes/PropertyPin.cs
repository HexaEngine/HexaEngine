namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImNodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
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
}