namespace HexaEngine.Materials.Pins
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Generator.Structs;
    using System.Text;

    public static class PinTypeHelper
    {
        public readonly static PinType[] NumericTypes =
        [
            PinType.Half,
            PinType.Half2,
            PinType.Half3,
            PinType.Half4,
            PinType.Float,
            PinType.Float2,
            PinType.Float3,
            PinType.Float4,
            PinType.Double,
            PinType.Double2,
            PinType.Double3,
            PinType.Double4,
            PinType.Bool,
            PinType.Bool2,
            PinType.Bool3,
            PinType.Bool4,
            PinType.Int,
            PinType.Int2,
            PinType.Int3,
            PinType.Int4,
            PinType.UInt,
            PinType.UInt2,
            PinType.UInt3,
            PinType.UInt4,
        ];

        public static string[] NumericTypesNames => GetNames(NumericTypes);

        public static string NumericTypesCombo => GetComboString(NumericTypes);

        public readonly static PinType[] NumericVectorOrScalarTypes =
        [
            PinType.Half,
            PinType.Half2OrHalf,
            PinType.Half3OrHalf,
            PinType.Half4OrHalf,
            PinType.Float,
            PinType.Float2OrFloat,
            PinType.Float3OrFloat,
            PinType.Float4OrFloat,
            PinType.Double,
            PinType.Double2OrDouble,
            PinType.Double3OrDouble,
            PinType.Double4OrDouble,
            PinType.Bool,
            PinType.Bool2OrBool,
            PinType.Bool3OrBool,
            PinType.Bool4OrBool,
            PinType.Int,
            PinType.Int2OrInt,
            PinType.Int3OrInt,
            PinType.Int4OrInt,
            PinType.UInt,
            PinType.UInt2OrUInt,
            PinType.UInt3OrUInt,
            PinType.UInt4OrUInt,
        ];

        public static string[] NumericVectorOrScalarTypesNames => GetNames(NumericVectorOrScalarTypes);

        public static string NumericVectorOrScalarTypesCombo => GetComboString(NumericVectorOrScalarTypes);

        public readonly static PinType[] NumericAnyTypes =
        [
            PinType.AnyHalf,
            PinType.AnyFloat,
            PinType.AnyDouble,
            PinType.AnyBool,
            PinType.AnyInt,
            PinType.AnyUInt,
        ];

        public static string[] NumericAnyTypesNames => GetNames(NumericAnyTypes);

        public static string NumericAnyTypesCombo => GetComboString(NumericAnyTypes);

        private static string[] GetNames<T>(T[] enums) where T : struct, Enum
        {
            string[] result = new string[enums.Length];
            for (int i = 0; i < enums.Length; i++)
            {
                result[i] = enums[i].ToString();
            }
            return result;
        }

        private static string GetComboString<T>(T[] enums) where T : struct, Enum
        {
            StringBuilder sb = new();
            for (int i = 0; i < enums.Length; i++)
            {
                sb.Append(enums[i]);
                sb.Append('\0');
            }
            sb.Append('\0');
            return sb.ToString();
        }

        public static int ComponentCount(this PinType type)
        {
            return type switch
            {
                PinType.DontCare => 0,
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
                _ => throw new NotSupportedException()
            };
        }

        public static bool IsNumeric(this PinType type)
        {
            return type switch
            {
                PinType.DontCare => true,
                PinType.Bool => true,
                PinType.Bool2 or PinType.Bool2OrBool => true,
                PinType.Bool3 or PinType.Bool3OrBool => true,
                PinType.Bool4 or PinType.Bool4OrBool => true,
                PinType.AnyBool => true,
                PinType.Double => true,
                PinType.Double2 or PinType.Double2OrDouble => true,
                PinType.Double3 or PinType.Double3OrDouble => true,
                PinType.Double4 or PinType.Double4OrDouble => true,
                PinType.AnyDouble => true,
                PinType.Float => true,
                PinType.Float2 or PinType.Float2OrFloat => true,
                PinType.Float3 or PinType.Float3OrFloat => true,
                PinType.Float4 or PinType.Float4OrFloat => true,
                PinType.AnyFloat => true,
                PinType.Half => true,
                PinType.Half2 or PinType.Half2OrHalf => true,
                PinType.Half3 or PinType.Half3OrHalf => true,
                PinType.Half4 or PinType.Half4OrHalf => true,
                PinType.AnyHalf => true,
                PinType.Int => true,
                PinType.Int2 or PinType.Int2OrInt => true,
                PinType.Int3 or PinType.Int3OrInt => true,
                PinType.Int4 or PinType.Int4OrInt => true,
                PinType.AnyInt => true,
                PinType.UInt => true,
                PinType.UInt2 or PinType.UInt2OrUInt => true,
                PinType.UInt3 or PinType.UInt3OrUInt => true,
                PinType.UInt4 or PinType.UInt4OrUInt => true,
                PinType.AnyUInt => true,
                _ => false,
            };
        }

        public static NumericType GetNumericType(this PinType type)
        {
            return type switch
            {
                PinType.DontCare => NumericType.Unknown,
                PinType.Bool => NumericType.Bool,
                PinType.Bool2 or PinType.Bool2OrBool => NumericType.Bool,
                PinType.Bool3 or PinType.Bool3OrBool => NumericType.Bool,
                PinType.Bool4 or PinType.Bool4OrBool => NumericType.Bool,
                PinType.AnyBool => NumericType.Bool,
                PinType.Double => NumericType.Double,
                PinType.Double2 or PinType.Double2OrDouble => NumericType.Double,
                PinType.Double3 or PinType.Double3OrDouble => NumericType.Double,
                PinType.Double4 or PinType.Double4OrDouble => NumericType.Double,
                PinType.AnyDouble => NumericType.Double,
                PinType.Float => NumericType.Float,
                PinType.Float2 or PinType.Float2OrFloat => NumericType.Float,
                PinType.Float3 or PinType.Float3OrFloat => NumericType.Float,
                PinType.Float4 or PinType.Float4OrFloat => NumericType.Float,
                PinType.AnyFloat => NumericType.Float,
                PinType.Half => NumericType.Half,
                PinType.Half2 or PinType.Half2OrHalf => NumericType.Half,
                PinType.Half3 or PinType.Half3OrHalf => NumericType.Half,
                PinType.Half4 or PinType.Half4OrHalf => NumericType.Half,
                PinType.AnyHalf => NumericType.Half,
                PinType.Int => NumericType.Int,
                PinType.Int2 or PinType.Int2OrInt => NumericType.Int,
                PinType.Int3 or PinType.Int3OrInt => NumericType.Int,
                PinType.Int4 or PinType.Int4OrInt => NumericType.Int,
                PinType.AnyInt => NumericType.Int,
                PinType.UInt => NumericType.UInt,
                PinType.UInt2 or PinType.UInt2OrUInt => NumericType.UInt,
                PinType.UInt3 or PinType.UInt3OrUInt => NumericType.UInt,
                PinType.UInt4 or PinType.UInt4OrUInt => NumericType.UInt,
                PinType.AnyUInt => NumericType.UInt,
                _ => throw new NotSupportedException()
            };
        }

        public static SType ToSType(this PinType pinType)
        {
            return pinType switch
            {
                PinType.Half => new(ScalarType.Half),
                PinType.Half2 or PinType.Half2OrHalf => new(VectorType.Half2),
                PinType.Half3 or PinType.Half3OrHalf => new(VectorType.Half3),
                PinType.Half4 or PinType.Half4OrHalf => new(VectorType.Half4),
                PinType.Float => new(ScalarType.Float),
                PinType.Float2 or PinType.Float2OrFloat => new(VectorType.Float2),
                PinType.Float3 or PinType.Float3OrFloat => new(VectorType.Float3),
                PinType.Float4 or PinType.Float4OrFloat => new(VectorType.Float4),
                PinType.Double => new(ScalarType.Double),
                PinType.Double2 or PinType.Double2OrDouble => new(VectorType.Double2),
                PinType.Double3 or PinType.Double3OrDouble => new(VectorType.Double3),
                PinType.Double4 or PinType.Double4OrDouble => new(VectorType.Double4),
                PinType.Bool => new(ScalarType.Bool),
                PinType.Bool2 or PinType.Bool2OrBool => new(VectorType.Bool2),
                PinType.Bool3 or PinType.Bool3OrBool => new(VectorType.Bool3),
                PinType.Bool4 or PinType.Bool4OrBool => new(VectorType.Bool4),
                PinType.Int => new(ScalarType.UInt),
                PinType.Int2 or PinType.Int2OrInt => new(VectorType.Int2),
                PinType.Int3 or PinType.Int3OrInt => new(VectorType.Int3),
                PinType.Int4 or PinType.Int4OrInt => new(VectorType.Int4),
                PinType.UInt => new(ScalarType.UInt),
                PinType.UInt2 or PinType.UInt2OrUInt => new(VectorType.UInt2),
                PinType.UInt3 or PinType.UInt3OrUInt => new(VectorType.UInt3),
                PinType.UInt4 or PinType.UInt4OrUInt => new(VectorType.UInt4),
                _ => throw new NotSupportedException($"Cannot convert '{pinType}' to SType."),
            };
        }

        public static PinType ToScalar(this PinType type)
        {
            return type switch
            {
                PinType.Bool => PinType.Bool,
                PinType.Bool2 or PinType.Bool2OrBool => PinType.Bool,
                PinType.Bool3 or PinType.Bool3OrBool => PinType.Bool,
                PinType.Bool4 or PinType.Bool4OrBool => PinType.Bool,
                PinType.AnyBool => PinType.Bool,
                PinType.Double => PinType.Double,
                PinType.Double2 or PinType.Double2OrDouble => PinType.Double,
                PinType.Double3 or PinType.Double3OrDouble => PinType.Double,
                PinType.Double4 or PinType.Double4OrDouble => PinType.Double,
                PinType.AnyDouble => PinType.Double,
                PinType.Float => PinType.Float,
                PinType.Float2 or PinType.Float2OrFloat => PinType.Float,
                PinType.Float3 or PinType.Float3OrFloat => PinType.Float,
                PinType.Float4 or PinType.Float4OrFloat => PinType.Float,
                PinType.AnyFloat => PinType.Float,
                PinType.Half => PinType.Half,
                PinType.Half2 or PinType.Half2OrHalf => PinType.Half,
                PinType.Half3 or PinType.Half3OrHalf => PinType.Half,
                PinType.Half4 or PinType.Half4OrHalf => PinType.Half,
                PinType.AnyHalf => PinType.Half,
                PinType.Int => PinType.Int,
                PinType.Int2 or PinType.Int2OrInt => PinType.Int,
                PinType.Int3 or PinType.Int3OrInt => PinType.Int,
                PinType.Int4 or PinType.Int4OrInt => PinType.Int,
                PinType.AnyInt => PinType.Int,
                PinType.UInt => PinType.UInt,
                PinType.UInt2 or PinType.UInt2OrUInt => PinType.UInt,
                PinType.UInt3 or PinType.UInt3OrUInt => PinType.UInt,
                PinType.UInt4 or PinType.UInt4OrUInt => PinType.UInt,
                PinType.AnyUInt => PinType.UInt,
                _ => throw new NotSupportedException($"Cannot convert '{type}' to scalar.")
            };
        }

        public static IEnumerable<PinType> GetSubTypes(this PinType type)
        {
            switch (type)
            {
                case PinType.Half2OrHalf:
                    yield return PinType.Half;
                    yield return PinType.Half2;
                    break;

                case PinType.Half3OrHalf:
                    yield return PinType.Half;
                    yield return PinType.Half3;
                    break;

                case PinType.Half4OrHalf:
                    yield return PinType.Half;
                    yield return PinType.Half4;
                    break;

                case PinType.Float2OrFloat:
                    yield return PinType.Float;
                    yield return PinType.Float2;
                    break;

                case PinType.Float3OrFloat:
                    yield return PinType.Float;
                    yield return PinType.Float3;
                    break;

                case PinType.Float4OrFloat:
                    yield return PinType.Float;
                    yield return PinType.Float4;
                    break;

                case PinType.Double2OrDouble:
                    yield return PinType.Double;
                    yield return PinType.Double2;
                    break;

                case PinType.Double3OrDouble:
                    yield return PinType.Double;
                    yield return PinType.Double3;
                    break;

                case PinType.Double4OrDouble:
                    yield return PinType.Double;
                    yield return PinType.Double4;
                    break;

                case PinType.Bool2OrBool:
                    yield return PinType.Bool;
                    yield return PinType.Bool2;
                    break;

                case PinType.Bool3OrBool:
                    yield return PinType.Bool;
                    yield return PinType.Bool3;
                    break;

                case PinType.Bool4OrBool:
                    yield return PinType.Bool;
                    yield return PinType.Bool4;
                    break;

                case PinType.Int2OrInt:
                    yield return PinType.Int;
                    yield return PinType.Int2;
                    break;

                case PinType.Int3OrInt:
                    yield return PinType.Int;
                    yield return PinType.Int3;
                    break;

                case PinType.Int4OrInt:
                    yield return PinType.Int;
                    yield return PinType.Int4;
                    break;

                case PinType.UInt2OrUInt:
                    yield return PinType.UInt;
                    yield return PinType.UInt2;
                    break;

                case PinType.UInt3OrUInt:
                    yield return PinType.UInt;
                    yield return PinType.UInt3;
                    break;

                case PinType.UInt4OrUInt:
                    yield return PinType.UInt;
                    yield return PinType.UInt4;
                    break;

                case PinType.AnyHalf:
                    yield return PinType.Half;
                    yield return PinType.Half2;
                    yield return PinType.Half3;
                    yield return PinType.Half4;
                    break;

                case PinType.AnyFloat:
                    yield return PinType.Float;
                    yield return PinType.Float2;
                    yield return PinType.Float3;
                    yield return PinType.Float4;
                    break;

                case PinType.AnyDouble:
                    yield return PinType.Double;
                    yield return PinType.Double2;
                    yield return PinType.Double3;
                    yield return PinType.Double4;
                    break;

                case PinType.AnyBool:
                    yield return PinType.Bool;
                    yield return PinType.Bool2;
                    yield return PinType.Bool3;
                    yield return PinType.Bool4;
                    break;

                case PinType.AnyInt:
                    yield return PinType.Int;
                    yield return PinType.Int2;
                    yield return PinType.Int3;
                    yield return PinType.Int4;
                    break;

                case PinType.AnyUInt:
                    yield return PinType.UInt;
                    yield return PinType.UInt2;
                    yield return PinType.UInt3;
                    yield return PinType.UInt4;
                    break;

                default:
                    yield return type;
                    break;
            }
        }
    }
}