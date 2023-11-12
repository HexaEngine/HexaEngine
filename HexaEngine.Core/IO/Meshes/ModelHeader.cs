namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents the header information of a model file.
    /// </summary>
    public struct ModelHeader
    {
        /// <summary>
        /// Magic number to identify the file format.
        /// </summary>
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x4d, 0x65, 0x73, 0x68, 0x00 };

        /// <summary>
        /// Current version of the model file format.
        /// </summary>
        public static readonly Version Version = 8;

        /// <summary>
        /// Minimum supported version of the model file format.
        /// </summary>
        public static readonly Version MinVersion = 7;

        /// <summary>
        /// Endianness of the data in the file.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// Text encoding used in the file.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// Compression method used for the data.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// Name of the material library associated with the model.
        /// </summary>
        public string MaterialLibrary;

        /// <summary>
        /// Number of meshes in the model.
        /// </summary>
        public ulong MeshCount;

        /// <summary>
        /// Position where the actual content starts in the file.
        /// </summary>
        public ulong ContentStart;

        /// <summary>
        /// Reads the header information from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the model file data.</param>
        public void Read(Stream stream)
        {
            // Check for the magic number to ensure correct file format
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException("Magic number mismatch");
            }

            // Read various properties from the stream
            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);
            MaterialLibrary = stream.ReadString(Encoding, Endianness) ?? string.Empty;
            MeshCount = stream.ReadUInt64(Endianness);

            // Set the content start position
            ContentStart = (ulong)stream.Position;
        }

        /// <summary>
        /// Writes the header information to a stream.
        /// </summary>
        /// <param name="stream">The stream to which the header data will be written.</param>
        public void Write(Stream stream)
        {
            // Write various properties to the stream
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteString(MaterialLibrary, Encoding, Endianness);
            stream.WriteUInt64(MeshCount, Endianness);
        }
    }
}