namespace HexaEngine.Core.IO.Binary.Textures.Raw
{
    using HexaEngine.Core.Graphics;
    using System.Buffers.Binary;

    /// <summary>
    /// Represents the header of a raw image data format, including width, height, and format information.
    /// </summary>
    public struct RawHeader
    {
        /// <summary>
        /// Gets or sets the width of the raw image in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets or sets the height of the raw image in pixels.
        /// </summary>
        public int Height;

        /// <summary>
        /// Gets or sets the format of the raw image data.
        /// </summary>
        public Format Format;

        /// <summary>
        /// Writes the raw image header to a specified stream in a binary format.
        /// </summary>
        /// <param name="dst">The stream to write the header to.</param>
        public readonly void Write(Stream dst)
        {
            Span<byte> buffer = stackalloc byte[12];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, Width);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..], Height);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[8..], (int)Format);
            dst.Write(buffer);
        }

        /// <summary>
        /// Reads the raw image header from a specified stream in a binary format.
        /// </summary>
        /// <param name="src">The stream to read the header from.</param>
        /// <returns>The parsed <see cref="RawHeader"/>.</returns>
        public static RawHeader ReadFrom(Stream src)
        {
            RawHeader header = default;
            header.Read(src);
            return header;
        }

        /// <summary>
        /// Reads the raw image header data from the given stream and populates the structure's fields.
        /// </summary>
        /// <param name="src">The stream to read the header data from.</param>
        public void Read(Stream src)
        {
            Span<byte> buffer = stackalloc byte[12];
            src.Read(buffer);
            Width = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            Height = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
            Format = (Format)BinaryPrimitives.ReadInt32LittleEndian(buffer[8..]);
        }
    }
}