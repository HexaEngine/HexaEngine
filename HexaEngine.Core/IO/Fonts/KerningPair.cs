namespace HexaEngine.Core.IO.Fonts
{
    using HexaEngine.Mathematics;

    /// <summary>
    /// Represents a kerning pair, consisting of two characters and an amount.
    /// </summary>
    public struct KerningPair
    {
        /// <summary>
        /// Gets or sets the first character of the kerning pair.
        /// </summary>
        public char First;

        /// <summary>
        /// Gets or sets the second character of the kerning pair.
        /// </summary>
        public char Second;

        /// <summary>
        /// Gets or sets the kerning amount for the pair.
        /// </summary>
        public uint Amount;

        /// <summary>
        /// Reads a <see cref="KerningPair"/> from the specified stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>The read <see cref="KerningPair"/>.</returns>
        public static KerningPair Read(Stream stream, Endianness endianness)
        {
            KerningPair kerningPair;
            kerningPair.First = (char)stream.ReadUInt16(endianness);
            kerningPair.Second = (char)stream.ReadUInt16(endianness);
            kerningPair.Amount = stream.ReadUInt32(endianness);
            return kerningPair;
        }

        /// <summary>
        /// Writes the <see cref="KerningPair"/> to the specified stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="endianness">The endianness to use for writing data to the stream.</param>
        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteUInt16(First, endianness);
            stream.WriteUInt16(Second, endianness);
            stream.WriteUInt32(Amount, endianness);
        }
    }
}