namespace HexaEngine.Core.Materials
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Pins;

    public static class PinHelper
    {
        public static Pin CreatePin(NodeEditor editor, SType type, string name, PinShape shape, PinKind kind, uint maxLinks = uint.MaxValue)
        {
            PinType pinType = PinType.DontCare;
            if (type.IsScalar)
            {
                pinType = type.ScalarType switch
                {
                    HexaEngine.Materials.Generator.Enums.ScalarType.Bool => PinType.Bool,
                    HexaEngine.Materials.Generator.Enums.ScalarType.Int => PinType.Int,
                    HexaEngine.Materials.Generator.Enums.ScalarType.UInt => PinType.UInt,
                    HexaEngine.Materials.Generator.Enums.ScalarType.Half => PinType.Half,
                    HexaEngine.Materials.Generator.Enums.ScalarType.Float => PinType.Float,
                    HexaEngine.Materials.Generator.Enums.ScalarType.Double => PinType.Double,
                    _ => PinType.DontCare,
                };
            }
            if (type.IsVector)
            {
                pinType = type.VectorType switch
                {
                    HexaEngine.Materials.Generator.Enums.VectorType.Bool2 => PinType.Bool2,
                    HexaEngine.Materials.Generator.Enums.VectorType.Bool3 => PinType.Bool3,
                    HexaEngine.Materials.Generator.Enums.VectorType.Bool4 => PinType.Bool4,
                    HexaEngine.Materials.Generator.Enums.VectorType.Int2 => PinType.Int2,
                    HexaEngine.Materials.Generator.Enums.VectorType.Int3 => PinType.Int3,
                    HexaEngine.Materials.Generator.Enums.VectorType.Int4 => PinType.Int4,
                    HexaEngine.Materials.Generator.Enums.VectorType.UInt2 => PinType.UInt2,
                    HexaEngine.Materials.Generator.Enums.VectorType.UInt3 => PinType.UInt3,
                    HexaEngine.Materials.Generator.Enums.VectorType.UInt4 => PinType.UInt4,
                    HexaEngine.Materials.Generator.Enums.VectorType.Half2 => PinType.Half2,
                    HexaEngine.Materials.Generator.Enums.VectorType.Half3 => PinType.Half3,
                    HexaEngine.Materials.Generator.Enums.VectorType.Half4 => PinType.Half4,
                    HexaEngine.Materials.Generator.Enums.VectorType.Float2 => PinType.Float2,
                    HexaEngine.Materials.Generator.Enums.VectorType.Float3 => PinType.Float3,
                    HexaEngine.Materials.Generator.Enums.VectorType.Float4 => PinType.Float4,
                    HexaEngine.Materials.Generator.Enums.VectorType.Double2 => PinType.Double2,
                    HexaEngine.Materials.Generator.Enums.VectorType.Double3 => PinType.Double3,
                    HexaEngine.Materials.Generator.Enums.VectorType.Double4 => PinType.Double4,
                    _ => PinType.DontCare,
                };
            }
            if (type.IsMatrix)
            {
                pinType = type.MatrixType switch
                {
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool1x1 => PinType.Bool1x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool1x2 => PinType.Bool1x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool1x3 => PinType.Bool1x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool1x4 => PinType.Bool1x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool2x1 => PinType.Bool2x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool2x2 => PinType.Bool2x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool2x3 => PinType.Bool2x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool2x4 => PinType.Bool2x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool3x1 => PinType.Bool3x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool3x2 => PinType.Bool3x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool3x3 => PinType.Bool3x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool3x4 => PinType.Bool3x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool4x1 => PinType.Bool4x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool4x2 => PinType.Bool4x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool4x3 => PinType.Bool4x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Bool4x4 => PinType.Bool4x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int1x1 => PinType.Int1x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int1x2 => PinType.Int1x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int1x3 => PinType.Int1x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int1x4 => PinType.Int1x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int2x1 => PinType.Int2x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int2x2 => PinType.Int2x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int2x3 => PinType.Int2x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int2x4 => PinType.Int2x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int3x1 => PinType.Int3x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int3x2 => PinType.Int3x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int3x3 => PinType.Int3x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int3x4 => PinType.Int3x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int4x1 => PinType.Int4x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int4x2 => PinType.Int4x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int4x3 => PinType.Int4x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Int4x4 => PinType.Int4x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt1x1 => PinType.UInt1x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt1x2 => PinType.UInt1x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt1x3 => PinType.UInt1x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt1x4 => PinType.UInt1x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt2x1 => PinType.UInt2x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt2x2 => PinType.UInt2x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt2x3 => PinType.UInt2x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt2x4 => PinType.UInt2x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt3x1 => PinType.UInt3x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt3x2 => PinType.UInt3x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt3x3 => PinType.UInt3x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt3x4 => PinType.UInt3x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt4x1 => PinType.UInt4x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt4x2 => PinType.UInt4x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt4x3 => PinType.UInt4x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.UInt4x4 => PinType.UInt4x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half1x1 => PinType.Half1x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half1x2 => PinType.Half1x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half1x3 => PinType.Half1x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half1x4 => PinType.Half1x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half2x1 => PinType.Half2x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half2x2 => PinType.Half2x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half2x3 => PinType.Half2x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half2x4 => PinType.Half2x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half3x1 => PinType.Half3x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half3x2 => PinType.Half3x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half3x3 => PinType.Half3x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half3x4 => PinType.Half3x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half4x1 => PinType.Half4x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half4x2 => PinType.Half4x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half4x3 => PinType.Half4x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Half4x4 => PinType.Half4x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float1x1 => PinType.Float1x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float1x2 => PinType.Float1x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float1x3 => PinType.Float1x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float1x4 => PinType.Float1x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float2x1 => PinType.Float2x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float2x2 => PinType.Float2x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float2x3 => PinType.Float2x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float2x4 => PinType.Float2x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float3x1 => PinType.Float3x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float3x2 => PinType.Float3x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float3x3 => PinType.Float3x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float3x4 => PinType.Float3x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float4x1 => PinType.Float4x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float4x2 => PinType.Float4x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float4x3 => PinType.Float4x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Float4x4 => PinType.Float4x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double1x1 => PinType.Double1x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double1x2 => PinType.Double1x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double1x3 => PinType.Double1x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double1x4 => PinType.Double1x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double2x1 => PinType.Double2x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double2x2 => PinType.Double2x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double2x3 => PinType.Double2x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double2x4 => PinType.Double2x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double3x1 => PinType.Double3x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double3x2 => PinType.Double3x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double3x3 => PinType.Double3x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double3x4 => PinType.Double3x4,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double4x1 => PinType.Double4x1,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double4x2 => PinType.Double4x2,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double4x3 => PinType.Double4x3,
                    HexaEngine.Materials.Generator.Enums.MatrixType.Double4x4 => PinType.Double4x4,
                    _ => PinType.DontCare
                };
            }

            return pinType switch
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
                _ => throw new NotSupportedException()
            };
        }
    }
}