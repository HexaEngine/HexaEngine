namespace HexaEngine.Editor.MaterialEditor.Pins
{
    using HexaEngine.Materials;

    public class PinTypeHelper
    {
        public static int ComponentCount(PinType type)
        {
            return type switch
            {
                PinType.Bool => 1,
                PinType.Bool2 or PinType.Bool2OrBool => 2,
                PinType.Bool3 or PinType.Bool3OrBool => 3,
                PinType.Bool4 or PinType.Bool4OrBool => 4,
                PinType.Double => 1,
                PinType.Double2 or PinType.Double2OrDouble => 2,
                PinType.Double3 or PinType.Double3OrDouble => 3,
                PinType.Double4 or PinType.Double4OrDouble => 4,
                PinType.Float => 1,
                PinType.Float2 or PinType.Float2OrFloat => 2,
                PinType.Float3 or PinType.Float3OrFloat => 3,
                PinType.Float4 or PinType.Float4OrFloat => 4,
                PinType.Half => 1,
                PinType.Half2 or PinType.Half2OrHalf => 2,
                PinType.Half3 or PinType.Half3OrHalf => 3,
                PinType.Half4 or PinType.Half4OrHalf => 4,
                PinType.Int => 1,
                PinType.Int2 or PinType.Int2OrInt => 2,
                PinType.Int3 or PinType.Int3OrInt => 3,
                PinType.Int4 or PinType.Int4OrInt => 4,
                PinType.UInt => 1,
                PinType.UInt2 or PinType.UInt2OrUInt => 2,
                PinType.UInt3 or PinType.UInt3OrUInt => 3,
                PinType.UInt4 or PinType.UInt4OrUInt => 4,
                _ => throw new NotSupportedException(),
            };
        }
    }
}