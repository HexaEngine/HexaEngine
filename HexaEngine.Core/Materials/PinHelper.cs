namespace HexaEngine.Core.Materials
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Pins;

    public static class PinHelper
    {
        public static PrimitivePin? CreatePin(NodeEditor editor, SType type, string name, PinShape shape, PinKind kind, uint maxLinks = uint.MaxValue)
        {
            PinType pinType = PinType.DontCare;
            if (type.IsScalar)
            {
                pinType = type.ScalarType switch
                {
                    ScalarType.Bool => PinType.Bool,
                    ScalarType.Int => PinType.Int,
                    ScalarType.UInt => PinType.UInt,
                    ScalarType.Half => PinType.Half,
                    ScalarType.Float => PinType.Float,
                    ScalarType.Double => PinType.Double,
                    _ => PinType.DontCare,
                };
            }
            if (type.IsVector)
            {
                pinType = type.VectorType switch
                {
                    VectorType.Bool2 => PinType.Bool2,
                    VectorType.Bool3 => PinType.Bool3,
                    VectorType.Bool4 => PinType.Bool4,
                    VectorType.Int2 => PinType.Int2,
                    VectorType.Int3 => PinType.Int3,
                    VectorType.Int4 => PinType.Int4,
                    VectorType.UInt2 => PinType.UInt2,
                    VectorType.UInt3 => PinType.UInt3,
                    VectorType.UInt4 => PinType.UInt4,
                    VectorType.Half2 => PinType.Half2,
                    VectorType.Half3 => PinType.Half3,
                    VectorType.Half4 => PinType.Half4,
                    VectorType.Float2 => PinType.Float2,
                    VectorType.Float3 => PinType.Float3,
                    VectorType.Float4 => PinType.Float4,
                    VectorType.Double2 => PinType.Double2,
                    VectorType.Double3 => PinType.Double3,
                    VectorType.Double4 => PinType.Double4,
                    _ => PinType.DontCare,
                };
            }
            if (type.IsMatrix)
            {
                pinType = type.MatrixType switch
                {
                    MatrixType.Bool1x1 => PinType.Bool1x1,
                    MatrixType.Bool1x2 => PinType.Bool1x2,
                    MatrixType.Bool1x3 => PinType.Bool1x3,
                    MatrixType.Bool1x4 => PinType.Bool1x4,
                    MatrixType.Bool2x1 => PinType.Bool2x1,
                    MatrixType.Bool2x2 => PinType.Bool2x2,
                    MatrixType.Bool2x3 => PinType.Bool2x3,
                    MatrixType.Bool2x4 => PinType.Bool2x4,
                    MatrixType.Bool3x1 => PinType.Bool3x1,
                    MatrixType.Bool3x2 => PinType.Bool3x2,
                    MatrixType.Bool3x3 => PinType.Bool3x3,
                    MatrixType.Bool3x4 => PinType.Bool3x4,
                    MatrixType.Bool4x1 => PinType.Bool4x1,
                    MatrixType.Bool4x2 => PinType.Bool4x2,
                    MatrixType.Bool4x3 => PinType.Bool4x3,
                    MatrixType.Bool4x4 => PinType.Bool4x4,
                    MatrixType.Int1x1 => PinType.Int1x1,
                    MatrixType.Int1x2 => PinType.Int1x2,
                    MatrixType.Int1x3 => PinType.Int1x3,
                    MatrixType.Int1x4 => PinType.Int1x4,
                    MatrixType.Int2x1 => PinType.Int2x1,
                    MatrixType.Int2x2 => PinType.Int2x2,
                    MatrixType.Int2x3 => PinType.Int2x3,
                    MatrixType.Int2x4 => PinType.Int2x4,
                    MatrixType.Int3x1 => PinType.Int3x1,
                    MatrixType.Int3x2 => PinType.Int3x2,
                    MatrixType.Int3x3 => PinType.Int3x3,
                    MatrixType.Int3x4 => PinType.Int3x4,
                    MatrixType.Int4x1 => PinType.Int4x1,
                    MatrixType.Int4x2 => PinType.Int4x2,
                    MatrixType.Int4x3 => PinType.Int4x3,
                    MatrixType.Int4x4 => PinType.Int4x4,
                    MatrixType.UInt1x1 => PinType.UInt1x1,
                    MatrixType.UInt1x2 => PinType.UInt1x2,
                    MatrixType.UInt1x3 => PinType.UInt1x3,
                    MatrixType.UInt1x4 => PinType.UInt1x4,
                    MatrixType.UInt2x1 => PinType.UInt2x1,
                    MatrixType.UInt2x2 => PinType.UInt2x2,
                    MatrixType.UInt2x3 => PinType.UInt2x3,
                    MatrixType.UInt2x4 => PinType.UInt2x4,
                    MatrixType.UInt3x1 => PinType.UInt3x1,
                    MatrixType.UInt3x2 => PinType.UInt3x2,
                    MatrixType.UInt3x3 => PinType.UInt3x3,
                    MatrixType.UInt3x4 => PinType.UInt3x4,
                    MatrixType.UInt4x1 => PinType.UInt4x1,
                    MatrixType.UInt4x2 => PinType.UInt4x2,
                    MatrixType.UInt4x3 => PinType.UInt4x3,
                    MatrixType.UInt4x4 => PinType.UInt4x4,
                    MatrixType.Half1x1 => PinType.Half1x1,
                    MatrixType.Half1x2 => PinType.Half1x2,
                    MatrixType.Half1x3 => PinType.Half1x3,
                    MatrixType.Half1x4 => PinType.Half1x4,
                    MatrixType.Half2x1 => PinType.Half2x1,
                    MatrixType.Half2x2 => PinType.Half2x2,
                    MatrixType.Half2x3 => PinType.Half2x3,
                    MatrixType.Half2x4 => PinType.Half2x4,
                    MatrixType.Half3x1 => PinType.Half3x1,
                    MatrixType.Half3x2 => PinType.Half3x2,
                    MatrixType.Half3x3 => PinType.Half3x3,
                    MatrixType.Half3x4 => PinType.Half3x4,
                    MatrixType.Half4x1 => PinType.Half4x1,
                    MatrixType.Half4x2 => PinType.Half4x2,
                    MatrixType.Half4x3 => PinType.Half4x3,
                    MatrixType.Half4x4 => PinType.Half4x4,
                    MatrixType.Float1x1 => PinType.Float1x1,
                    MatrixType.Float1x2 => PinType.Float1x2,
                    MatrixType.Float1x3 => PinType.Float1x3,
                    MatrixType.Float1x4 => PinType.Float1x4,
                    MatrixType.Float2x1 => PinType.Float2x1,
                    MatrixType.Float2x2 => PinType.Float2x2,
                    MatrixType.Float2x3 => PinType.Float2x3,
                    MatrixType.Float2x4 => PinType.Float2x4,
                    MatrixType.Float3x1 => PinType.Float3x1,
                    MatrixType.Float3x2 => PinType.Float3x2,
                    MatrixType.Float3x3 => PinType.Float3x3,
                    MatrixType.Float3x4 => PinType.Float3x4,
                    MatrixType.Float4x1 => PinType.Float4x1,
                    MatrixType.Float4x2 => PinType.Float4x2,
                    MatrixType.Float4x3 => PinType.Float4x3,
                    MatrixType.Float4x4 => PinType.Float4x4,
                    MatrixType.Double1x1 => PinType.Double1x1,
                    MatrixType.Double1x2 => PinType.Double1x2,
                    MatrixType.Double1x3 => PinType.Double1x3,
                    MatrixType.Double1x4 => PinType.Double1x4,
                    MatrixType.Double2x1 => PinType.Double2x1,
                    MatrixType.Double2x2 => PinType.Double2x2,
                    MatrixType.Double2x3 => PinType.Double2x3,
                    MatrixType.Double2x4 => PinType.Double2x4,
                    MatrixType.Double3x1 => PinType.Double3x1,
                    MatrixType.Double3x2 => PinType.Double3x2,
                    MatrixType.Double3x3 => PinType.Double3x3,
                    MatrixType.Double3x4 => PinType.Double3x4,
                    MatrixType.Double4x1 => PinType.Double4x1,
                    MatrixType.Double4x2 => PinType.Double4x2,
                    MatrixType.Double4x3 => PinType.Double4x3,
                    MatrixType.Double4x4 => PinType.Double4x4,
                    _ => PinType.DontCare
                };
            }

            return CreatePin(editor, name, shape, kind, maxLinks, pinType);
        }

        public static PrimitivePin? CreatePin(NodeEditor editor, string name, PinShape shape, PinKind kind, uint maxLinks, PinType pinType)
        {
            var pin = pinType switch
            {
                PinType.Half2OrHalf => PinType.Half2,
                PinType.Half3OrHalf => PinType.Half3,
                PinType.Half4OrHalf => PinType.Half4,
                PinType.Float2OrFloat => PinType.Float2,
                PinType.Float3OrFloat => PinType.Float3,
                PinType.Float4OrFloat => PinType.Float4,
                PinType.Double2OrDouble => PinType.Double2,
                PinType.Double3OrDouble => PinType.Double3,
                PinType.Double4OrDouble => PinType.Double4,
                PinType.Bool2OrBool => PinType.Bool2,
                PinType.Bool3OrBool => PinType.Bool3,
                PinType.Bool4OrBool => PinType.Bool4,
                PinType.Int2OrInt => PinType.Int2,
                PinType.Int3OrInt => PinType.Int3,
                PinType.Int4OrInt => PinType.Int4,
                PinType.UInt2OrUInt => PinType.UInt2,
                PinType.UInt3OrUInt => PinType.UInt3,
                PinType.UInt4OrUInt => PinType.UInt4,
                _ => pinType,
            };

            return pin switch
            {
                PinType.Bool or PinType.Bool2 or PinType.Bool3 or PinType.Bool4 => new BoolPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Int or PinType.Int2 or PinType.Int3 or PinType.Int4 => new IntPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.UInt or PinType.UInt2 or PinType.UInt3 or PinType.UInt4 => new UIntPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks: maxLinks),
                PinType.Half or PinType.Half2 or PinType.Half3 or PinType.Half4 => new HalfPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Float or PinType.Float2 or PinType.Float3 or PinType.Float4 => new FloatPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Double or PinType.Double2 or PinType.Double3 or PinType.Double4 => new DoublePin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Bool1x1 or PinType.Bool1x2 or PinType.Bool1x3 or PinType.Bool1x4 or PinType.Bool2x1 or PinType.Bool2x2 or PinType.Bool2x3 or PinType.Bool2x4 or PinType.Bool3x1 or PinType.Bool3x2 or PinType.Bool3x3 or PinType.Bool3x4 or PinType.Bool4x1 or PinType.Bool4x2 or PinType.Bool4x3 or PinType.Bool4x4 => new BoolMatrixPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Int1x1 or PinType.Int1x2 or PinType.Int1x3 or PinType.Int1x4 or PinType.Int2x1 or PinType.Int2x2 or PinType.Int2x3 or PinType.Int2x4 or PinType.Int3x1 or PinType.Int3x2 or PinType.Int3x3 or PinType.Int3x4 or PinType.Int4x1 or PinType.Int4x2 or PinType.Int4x3 or PinType.Int4x4 => new IntMatrixPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.UInt1x1 or PinType.UInt1x2 or PinType.UInt1x3 or PinType.UInt1x4 or PinType.UInt2x1 or PinType.UInt2x2 or PinType.UInt2x3 or PinType.UInt2x4 or PinType.UInt3x1 or PinType.UInt3x2 or PinType.UInt3x3 or PinType.UInt3x4 or PinType.UInt4x1 or PinType.UInt4x2 or PinType.UInt4x3 or PinType.UInt4x4 => new UIntMatrixPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Half1x1 or PinType.Half1x2 or PinType.Half1x3 or PinType.Half1x4 or PinType.Half2x1 or PinType.Half2x2 or PinType.Half2x3 or PinType.Half2x4 or PinType.Half3x1 or PinType.Half3x2 or PinType.Half3x3 or PinType.Half3x4 or PinType.Half4x1 or PinType.Half4x2 or PinType.Half4x3 or PinType.Half4x4 => new HalfMatrixPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Float1x1 or PinType.Float1x2 or PinType.Float1x3 or PinType.Float1x4 or PinType.Float2x1 or PinType.Float2x2 or PinType.Float2x3 or PinType.Float2x4 or PinType.Float3x1 or PinType.Float3x2 or PinType.Float3x3 or PinType.Float3x4 or PinType.Float4x1 or PinType.Float4x2 or PinType.Float4x3 or PinType.Float4x4 => new FloatMatrixPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                PinType.Double1x1 or PinType.Double1x2 or PinType.Double1x3 or PinType.Double1x4 or PinType.Double2x1 or PinType.Double2x2 or PinType.Double2x3 or PinType.Double2x4 or PinType.Double3x1 or PinType.Double3x2 or PinType.Double3x3 or PinType.Double3x4 or PinType.Double4x1 or PinType.Double4x2 or PinType.Double4x3 or PinType.Double4x4 => new DoubleMatrixPin(editor.GetUniqueId(), name, shape, kind, pinType, maxLinks),
                _ => null
            };
        }
    }
}