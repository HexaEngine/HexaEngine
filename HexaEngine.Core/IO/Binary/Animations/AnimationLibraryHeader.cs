namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents the header information of an animation library.
    /// </summary>
    public struct AnimationLibraryHeader
    {
        /// <summary>
        /// Magic number used to identify the animation library file format.
        /// </summary>
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6e, 0x73, 0x41, 0x6e, 0x69, 0x6d, 0x61, 0x74, 0x6F, 0x6E, 0x00];

        /// <summary>
        /// The current version of the animation library header.
        /// </summary>
        public static readonly Version Version = 2;

        /// <summary>
        /// The minimum supported version of the animation library header.
        /// </summary>
        public static readonly Version MinVersion = 2;

        /// <summary>
        /// The endianness of binary data.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The encoding used for writing strings.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// The compression method used for data in the animation library.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// The number of animations in the library.
        /// </summary>
        public int AnimationCount;

        /// <summary>
        /// Reads the header information from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the header from.</param>
        /// <exception cref="InvalidDataException">Thrown if the magic number or version doesn't match.</exception>
        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException("Magic number mismatch");
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);
            AnimationCount = stream.ReadInt32(Endianness);
        }

        /// <summary>
        /// Writes the header information to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the header to.</param>
        public readonly void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteInt32(AnimationCount, Endianness);
        }
    }
}