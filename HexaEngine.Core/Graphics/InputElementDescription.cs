namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.IO;
    using Hexa.NET.Mathematics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Describes an element in the layout of a vertex buffer.
    /// </summary>
    public struct InputElementDescription : IEquatable<InputElementDescription>
    {
        /// <summary>
        /// The semantic associated with this element in a shader input-signature.
        /// </summary>
        public string SemanticName;

        /// <summary>
        /// The semantic index for the element. A semantic index modifies a semantic, with an integer index number. A semantic index is only needed in a case where there is more than one element with the same semantic.
        /// </summary>
        public int SemanticIndex;

        /// <summary>
        /// The format that specifies the format of the element data.
        /// </summary>
        public Format Format;

        /// <summary>
        /// The input-assembler slot.
        /// </summary>
        public int Slot;

        /// <summary>
        /// Offset, in bytes, between each element. Use <see cref="AppendAligned"/> (0xffffffff) for convenience to define the current element directly after the previous one, including any packing if necessary.
        /// </summary>
        public int AlignedByteOffset;

        /// <summary>
        /// The input data class for a single input slot.
        /// </summary>
        public InputClassification Classification;

        /// <summary>
        /// The number of instances to draw using the same per-instance data before advancing in the buffer by one element. This value must be 0 for an element that contains per-vertex data (the slot class is set to <see cref="InputClassification.PerVertexData"/>).
        /// </summary>
        public int InstanceDataStepRate;

        /// <summary>
        /// A special value to specify that the element should be appended directly after the previous one, including any required packing.
        /// </summary>
        public const int AppendAligned = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementDescription"/> struct.
        /// </summary>
        /// <param name="semanticName">The semantic associated with this element in a shader input-signature.</param>
        /// <param name="semanticIndex">The semantic index for the element.</param>
        /// <param name="format">The format that specifies the format of the element data.</param>
        /// <param name="offset">Offset, in bytes, between each element.</param>
        /// <param name="slot">The input-assembler slot.</param>
        /// <param name="slotClass">The input data class for a single input slot.</param>
        /// <param name="stepRate">The number of instances to draw using the same per-instance data before advancing in the buffer by one element.</param>
        public InputElementDescription(string semanticName, int semanticIndex, Format format, int offset, int slot, InputClassification slotClass, int stepRate)
        {
            SemanticName = semanticName;
            SemanticIndex = semanticIndex;
            Format = format;
            Slot = slot;
            AlignedByteOffset = offset;
            Classification = slotClass;
            InstanceDataStepRate = stepRate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementDescription"/> struct.
        /// </summary>
        /// <param name="semanticName">The HLSL semantic associated with this element in a shader input-signature.</param>
        /// <param name="semanticIndex">The semantic index for the element.</param>
        /// <param name="format">The format that specifies the format of the element data.</param>
        /// <param name="offset">Offset, in bytes, between each element. Use <see cref="AppendAligned"/> (0xffffffff) for convenience to define the current element directly after the previous one, including any packing if necessary.</param>
        /// <param name="slot">The input-assembler slot.</param>
        public InputElementDescription(string semanticName, int semanticIndex, Format format, int offset, int slot)
        {
            SemanticName = semanticName;
            SemanticIndex = semanticIndex;
            Format = format;
            Slot = slot;
            AlignedByteOffset = offset;
            Classification = InputClassification.PerVertexData;
            InstanceDataStepRate = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementDescription"/> struct.
        /// </summary>
        /// <param name="semanticName">The HLSL semantic associated with this element in a shader input-signature.</param>
        /// <param name="semanticIndex">The semantic index for the element.</param>
        /// <param name="format">The format that specifies the format of the element data.</param>
        /// <param name="slot">The input-assembler slot.</param>
        public InputElementDescription(string semanticName, int semanticIndex, Format format, int slot)
        {
            SemanticName = semanticName;
            SemanticIndex = semanticIndex;
            Format = format;
            Slot = slot;
            AlignedByteOffset = AppendAligned;
            Classification = InputClassification.PerVertexData;
            InstanceDataStepRate = 0;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is InputElementDescription value && Equals(value);

        /// <summary>
        /// Determines whether the specified <see cref="InputElementDescription"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="InputElementDescription"/> to compare with this instance.</param>
        public readonly bool Equals(InputElementDescription other)
        {
            return SemanticName == other.SemanticName &&
                   SemanticIndex == other.SemanticIndex &&
                   Format == other.Format &&
                   Slot == other.Slot &&
                   AlignedByteOffset == other.AlignedByteOffset &&
                   Classification == other.Classification &&
                   InstanceDataStepRate == other.InstanceDataStepRate;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(SemanticName, SemanticIndex, Format, Slot, AlignedByteOffset, Classification, InstanceDataStepRate);
        }

        /// <summary>
        /// Compares two <see cref="InputElementDescription"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="InputElementDescription"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="InputElementDescription"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        public static bool operator ==(InputElementDescription left, InputElementDescription right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="InputElementDescription"/> objects for inequality.
        /// </summary>
        /// <param name="left">The <see cref="InputElementDescription"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="InputElementDescription"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        public static bool operator !=(InputElementDescription left, InputElementDescription right) => !left.Equals(right);

        /// <summary>
        /// Writes the elements of this <see cref="InputElementDescription"/> to a binary stream.
        /// </summary>
        /// <param name="stream">The binary stream to which the data is written.</param>
        /// <param name="encoding">The character encoding to use when writing the semantic name.</param>
        /// <param name="endianness">The byte order to use when writing the binary data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(SemanticName, encoding, endianness);
            stream.WriteInt32(SemanticIndex, endianness);
            stream.WriteInt32((int)Format, endianness);
            stream.WriteInt32(Slot, endianness);
            stream.WriteInt32(AlignedByteOffset, endianness);
            stream.WriteInt32((int)Classification, endianness);
            stream.WriteInt32(InstanceDataStepRate, endianness);
        }

        /// <summary>
        /// Reads the elements of this <see cref="InputElementDescription"/> from a binary stream.
        /// </summary>
        /// <param name="stream">The binary stream from which the data is read.</param>
        /// <param name="encoding">The character encoding to use when reading the semantic name.</param>
        /// <param name="endianness">The byte order to use when reading the binary data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            SemanticName = stream.ReadString(encoding, endianness) ?? string.Empty;
            SemanticIndex = stream.ReadInt32(endianness);
            Format = (Format)stream.ReadInt32(endianness);
            Slot = stream.ReadInt32(endianness);
            AlignedByteOffset = stream.ReadInt32(endianness);
            Classification = (InputClassification)stream.ReadInt32(endianness);
            InstanceDataStepRate = stream.ReadInt32(endianness);
        }
    }
}