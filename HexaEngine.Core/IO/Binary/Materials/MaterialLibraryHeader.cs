namespace HexaEngine.Core.IO.Binary.Materials
{
    using Hexa.NET.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents the header structure of a material library file.
    /// </summary>
    public struct MaterialLibraryHeader
    {
        /// <summary>
        /// Magic number used to identify the file format.
        /// </summary>
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x4D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x00];

        /// <summary>
        /// Current version of the material library file format.
        /// </summary>
        public static readonly Version Version = new(6, 0, 0, 1);

        /// <summary>
        /// Minimum supported version of the material library file format.
        /// </summary>
        public static readonly Version MinVersion = new(5, 0, 0, 0);

        /// <summary>
        /// The endianness of the material library file.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The encoding used for strings in the material library.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// The compression method used for the material library data.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// The number of materials in the material library.
        /// </summary>
        public uint MaterialCount;

        /// <summary>
        /// Reads the material library header from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the header from.</param>
        /// <param name="version"></param>
        /// <exception cref="InvalidDataException">Thrown if the magic number is not found or if there is a version mismatch.</exception>
        public void Read(Stream stream, out Version version)
        {
            version = default;
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException();
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);
            MaterialCount = stream.ReadUInt32(Endianness);
        }

        /// <summary>
        /// Writes the material library header to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write the header to.</param>
        public readonly void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteUInt32(MaterialCount, Endianness);
        }
    }
}