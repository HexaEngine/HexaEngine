namespace HexaEngine.Materials.Pins
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Materials;
    using System.Diagnostics;
    using System.Globalization;
    using System.Numerics;

    public class UniversalPin : NumericPin
    {
        [JsonIgnore] public readonly string name;
        [JsonIgnore] public readonly string nameY;
        [JsonIgnore] public readonly string nameZ;
        [JsonIgnore] public readonly string nameW;
        [JsonIgnore] public bool changing;

        public BoolVector4 boolVector;
        public HalfVector4 halfVector;
        public Vector4 floatVector;
        public Vector4D doubleVector;
        public Point4 intVector;
        public UPoint4 uintVector;

        public UniversalPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.Flags = flags;
            SanityChecks();
        }

        private void SanityChecks()
        {
            switch (PinTypeHelper.GetNumericType(Type))
            {
                case NumericType.Bool:
                    Trace.Assert(Type == PinType.Bool || Type == PinType.Bool2 || Type == PinType.Bool3 || Type == PinType.Bool4 || Type == PinType.Bool2OrBool || Type == PinType.Bool3OrBool || Type == PinType.Bool4OrBool, $"PinType {Type} is not a bool!");
                    Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0 || (Flags & PinFlags.InferType) != 0, $"PinFlags {Flags} is not supported!");
                    Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0 && (Flags & PinFlags.Slider) == 0, $"ColorPicker and ColorEdit is not supported!");
                    break;

                case NumericType.Half:
                    Trace.Assert(Type == PinType.Half || Type == PinType.Half2 || Type == PinType.Half3 || Type == PinType.Half4 || Type == PinType.Half2OrHalf || Type == PinType.Half3OrHalf || Type == PinType.Half4OrHalf, $"PinType {Type} is not a float!");
                    Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0 || (Flags & PinFlags.InferType) != 0, $"PinFlags {Flags} is not supported!");
                    break;

                case NumericType.Float:
                    Trace.Assert(Type == PinType.Float || Type == PinType.Float2 || Type == PinType.Float3 || Type == PinType.Float4 || Type == PinType.Float2OrFloat || Type == PinType.Float3OrFloat || Type == PinType.Float4OrFloat, $"PinType {Type} is not a float!");
                    Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0 || (Flags & PinFlags.InferType) != 0, $"PinFlags {Flags} is not supported!");
                    break;

                case NumericType.Double:
                    Trace.Assert(Type == PinType.Double || Type == PinType.Double2 || Type == PinType.Double3 || Type == PinType.Double4 || Type == PinType.Double2OrDouble || Type == PinType.Double3OrDouble || Type == PinType.Double4OrDouble, $"PinType {Type} is not a double!");
                    Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0 || (Flags & PinFlags.InferType) != 0, $"PinFlags {Flags} is not supported!");
                    Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0, $"ColorPicker and ColorEdit is not supported!");
                    break;

                case NumericType.Int:
                    Trace.Assert(Type == PinType.Int || Type == PinType.Int2 || Type == PinType.Int3 || Type == PinType.Int4 || Type == PinType.Int2OrInt || Type == PinType.Int3OrInt || Type == PinType.Int4OrInt, $"PinType {Type} is not a int!");
                    Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0 || (Flags & PinFlags.InferType) != 0, $"PinFlags {Flags} is not supported!");
                    Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0, $"ColorPicker and ColorEdit is not supported!");
                    break;

                case NumericType.UInt:
                    Trace.Assert(Type == PinType.UInt || Type == PinType.UInt2 || Type == PinType.UInt3 || Type == PinType.UInt4 || Type == PinType.UInt2OrUInt || Type == PinType.UInt3OrUInt || Type == PinType.UInt4OrUInt, $"PinType {Type} is not a uint!");
                    Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0 || (Flags & PinFlags.InferType) != 0, $"PinFlags {Flags} is not supported!");
                    Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0, $"ColorPicker and ColorEdit is not supported!");
                    break;
            }
        }

        public override string GetDefaultValue()
        {
            return Type switch
            {
                PinType.Bool => boolVector.X.ToString(CultureInfo.InvariantCulture),
                PinType.Bool2 => $"bool2({boolVector.X.ToString(CultureInfo.InvariantCulture)},{boolVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool3 => $"bool3({boolVector.X.ToString(CultureInfo.InvariantCulture)},{boolVector.Y.ToString(CultureInfo.InvariantCulture)},{boolVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool4 => $"bool4({boolVector.X.ToString(CultureInfo.InvariantCulture)},{boolVector.Y.ToString(CultureInfo.InvariantCulture)},{boolVector.Z.ToString(CultureInfo.InvariantCulture)},{boolVector.W.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool2OrBool => $"bool2({boolVector.X.ToString(CultureInfo.InvariantCulture)},{boolVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool3OrBool => $"bool3({boolVector.X.ToString(CultureInfo.InvariantCulture)},{boolVector.Y.ToString(CultureInfo.InvariantCulture)},{boolVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool4OrBool => $"bool4({boolVector.X.ToString(CultureInfo.InvariantCulture)},{boolVector.Y.ToString(CultureInfo.InvariantCulture)},{boolVector.Z.ToString(CultureInfo.InvariantCulture)},{boolVector.W.ToString(CultureInfo.InvariantCulture)})",

                PinType.Half => halfVector.X.ToString(CultureInfo.InvariantCulture),
                PinType.Half2 => $"half2({halfVector.X.ToString(CultureInfo.InvariantCulture)},{halfVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half3 => $"half3({halfVector.X.ToString(CultureInfo.InvariantCulture)},{halfVector.Y.ToString(CultureInfo.InvariantCulture)},{halfVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half4 => $"half4({halfVector.X.ToString(CultureInfo.InvariantCulture)},{halfVector.Y.ToString(CultureInfo.InvariantCulture)},{halfVector.Z.ToString(CultureInfo.InvariantCulture)},{halfVector.W.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half2OrHalf => $"half2({halfVector.X.ToString(CultureInfo.InvariantCulture)},{halfVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half3OrHalf => $"half3({halfVector.X.ToString(CultureInfo.InvariantCulture)},{halfVector.Y.ToString(CultureInfo.InvariantCulture)},{halfVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half4OrHalf => $"half4({halfVector.X.ToString(CultureInfo.InvariantCulture)},{halfVector.Y.ToString(CultureInfo.InvariantCulture)},{halfVector.Z.ToString(CultureInfo.InvariantCulture)},{halfVector.W.ToString(CultureInfo.InvariantCulture)})",

                PinType.Float => floatVector.X.ToString(CultureInfo.InvariantCulture),
                PinType.Float2 => $"float2({floatVector.X.ToString(CultureInfo.InvariantCulture)},{floatVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float3 => $"float3({floatVector.X.ToString(CultureInfo.InvariantCulture)},{floatVector.Y.ToString(CultureInfo.InvariantCulture)},{floatVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float4 => $"float4({floatVector.X.ToString(CultureInfo.InvariantCulture)},{floatVector.Y.ToString(CultureInfo.InvariantCulture)},{floatVector.Z.ToString(CultureInfo.InvariantCulture)},{floatVector.W.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float2OrFloat => $"float2({floatVector.X.ToString(CultureInfo.InvariantCulture)},{floatVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float3OrFloat => $"float3({floatVector.X.ToString(CultureInfo.InvariantCulture)},{floatVector.Y.ToString(CultureInfo.InvariantCulture)},{floatVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float4OrFloat => $"float4({floatVector.X.ToString(CultureInfo.InvariantCulture)},{floatVector.Y.ToString(CultureInfo.InvariantCulture)},{floatVector.Z.ToString(CultureInfo.InvariantCulture)},{floatVector.W.ToString(CultureInfo.InvariantCulture)})",

                PinType.Double => doubleVector.X.ToString(CultureInfo.InvariantCulture),
                PinType.Double2 => $"double2({doubleVector.X.ToString(CultureInfo.InvariantCulture)},{doubleVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double3 => $"double3({doubleVector.X.ToString(CultureInfo.InvariantCulture)},{doubleVector.Y.ToString(CultureInfo.InvariantCulture)},{doubleVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double4 => $"double4({doubleVector.X.ToString(CultureInfo.InvariantCulture)},{doubleVector.Y.ToString(CultureInfo.InvariantCulture)},{doubleVector.Z.ToString(CultureInfo.InvariantCulture)},{doubleVector.W.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double2OrDouble => $"double2({doubleVector.X.ToString(CultureInfo.InvariantCulture)},{doubleVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double3OrDouble => $"double3({doubleVector.X.ToString(CultureInfo.InvariantCulture)},{doubleVector.Y.ToString(CultureInfo.InvariantCulture)},{doubleVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double4OrDouble => $"double4({doubleVector.X.ToString(CultureInfo.InvariantCulture)},{doubleVector.Y.ToString(CultureInfo.InvariantCulture)},{doubleVector.Z.ToString(CultureInfo.InvariantCulture)},{doubleVector.W.ToString(CultureInfo.InvariantCulture)})",

                PinType.Int => intVector.X.ToString(CultureInfo.InvariantCulture),
                PinType.Int2 => $"int2({intVector.X.ToString(CultureInfo.InvariantCulture)},{intVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int3 => $"int3({intVector.X.ToString(CultureInfo.InvariantCulture)},{intVector.Y.ToString(CultureInfo.InvariantCulture)},{intVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int4 => $"int4({intVector.X.ToString(CultureInfo.InvariantCulture)},{intVector.Y.ToString(CultureInfo.InvariantCulture)},{intVector.Z.ToString(CultureInfo.InvariantCulture)},{intVector.W.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int2OrInt => $"int2({intVector.X.ToString(CultureInfo.InvariantCulture)},{intVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int3OrInt => $"int3({intVector.X.ToString(CultureInfo.InvariantCulture)},{intVector.Y.ToString(CultureInfo.InvariantCulture)},{intVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int4OrInt => $"int4({intVector.X.ToString(CultureInfo.InvariantCulture)},{intVector.Y.ToString(CultureInfo.InvariantCulture)},{intVector.Z.ToString(CultureInfo.InvariantCulture)},{intVector.W.ToString(CultureInfo.InvariantCulture)})",

                PinType.UInt => uintVector.X.ToString(CultureInfo.InvariantCulture),
                PinType.UInt2 => $"uint2({uintVector.X.ToString(CultureInfo.InvariantCulture)},{uintVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.UInt3 => $"uint3({uintVector.X.ToString(CultureInfo.InvariantCulture)},{uintVector.Y.ToString(CultureInfo.InvariantCulture)},{uintVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.UInt4 => $"uint4({uintVector.X.ToString(CultureInfo.InvariantCulture)},{uintVector.Y.ToString(CultureInfo.InvariantCulture)},{uintVector.Z.ToString(CultureInfo.InvariantCulture)},{uintVector.W.ToString(CultureInfo.InvariantCulture)})",
                PinType.UInt2OrUInt => $"uint2({uintVector.X.ToString(CultureInfo.InvariantCulture)},{uintVector.Y.ToString(CultureInfo.InvariantCulture)})",
                PinType.UInt3OrUInt => $"uint3({uintVector.X.ToString(CultureInfo.InvariantCulture)},{uintVector.Y.ToString(CultureInfo.InvariantCulture)},{uintVector.Z.ToString(CultureInfo.InvariantCulture)})",
                PinType.UInt4OrUInt => $"uint4({uintVector.X.ToString(CultureInfo.InvariantCulture)},{uintVector.Y.ToString(CultureInfo.InvariantCulture)},{uintVector.Z.ToString(CultureInfo.InvariantCulture)},{uintVector.W.ToString(CultureInfo.InvariantCulture)})",

                _ => "0",
            };
        }
    }
}