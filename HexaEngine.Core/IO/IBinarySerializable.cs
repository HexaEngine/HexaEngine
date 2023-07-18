namespace HexaEngine.Core.IO
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents an interface for binary serialization and deserialization.
    /// </summary>
    public interface IBinarySerializable
    {
        /// <summary>
        /// Reads the binary data from the specified source span and decodes it using the specified encoding.
        /// </summary>
        /// <param name="src">The source span containing the binary data.</param>
        /// <param name="encoding">The encoding used to decode the binary data.</param>
        /// <returns>The number of bytes read from the source span.</returns>
        int Read(ReadOnlySpan<byte> src, Encoding encoding);

        /// <summary>
        /// Reads the binary data from the specified stream and decodes it using the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to read the binary data from.</param>
        /// <param name="encoding">The encoding used to decode the binary data.</param>
        void Read(Stream stream, Encoding encoding);

        /// <summary>
        /// Calculates the size in bytes of the binary data when serialized using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding used for serialization.</param>
        /// <returns>The size in bytes of the serialized binary data.</returns>
        int Size(Encoding encoding);

        /// <summary>
        /// Writes the serialized binary data to the specified destination span using the specified encoding.
        /// </summary>
        /// <param name="dst">The destination span to write the binary data to.</param>
        /// <param name="encoding">The encoding used for serialization.</param>
        /// <returns>The number of bytes written to the destination span.</returns>
        int Write(Span<byte> dst, Encoding encoding);

        /// <summary>
        /// Writes the serialized binary data to the specified stream using the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to write the binary data to.</param>
        /// <param name="encoding">The encoding used for serialization.</param>
        void Write(Stream stream, Encoding encoding);
    }
}