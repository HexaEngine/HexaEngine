namespace HexaEngine.Core.IO.Binary.Meshes
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
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6e, 0x73, 0x4d, 0x65, 0x73, 0x68, 0x00];

        /// <summary>
        /// Current version of the model file format.
        /// </summary>
        public static readonly Version Version = new(11, 0, 0, 1);

        /// <summary>
        /// Minimum supported version of the model file format.
        /// </summary>
        public static readonly Version MinVersion = new(11, 0, 0, 0);

        /// <summary>
        /// Endianness of the data in the file.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The file format version of the current file.
        /// </summary>
        public Version FileVersion;

        /// <summary>
        /// Text encoding used in the file.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// Compression method used for the data.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// Model file flags.
        /// </summary>
        public ModelFlags ModelFlags;

        /// <summary>
        /// Name of the material library associated with the model.
        /// </summary>
        [Obsolete("Not longer available after Version 11.0.0.1")]
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

            FileVersion = version;

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);

            if (version == new Version(11, 0, 0, 0))
            {
                MaterialLibrary = stream.ReadString(Encoding, Endianness) ?? string.Empty;
            }
            else if (version == Version)
            {
                ModelFlags = (ModelFlags)stream.ReadInt32(Endianness);
            }

            MeshCount = stream.ReadUInt64(Endianness);

            // Set the content start position
            ContentStart = (ulong)stream.Position;
        }

        /// <summary>
        /// Reads the header information from a stream.
        /// </summary>
        /// <param name="stream">The stream containing the model file data.</param>
        public bool TryRead(Stream stream)
        {
            // Check for the magic number to ensure correct file format
            if (!stream.Compare(MagicNumber))
            {
                return false;
            }

            // Read various properties from the stream
            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                return false;
            }

            FileVersion = version;

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);

            if (version == new Version(11, 0, 0, 0))
            {
                MaterialLibrary = stream.ReadString(Encoding, Endianness) ?? string.Empty;
            }
            else if (version == Version)
            {
                ModelFlags = (ModelFlags)stream.ReadInt32(Endianness);
            }

            MeshCount = stream.ReadUInt64(Endianness);

            // Set the content start position
            ContentStart = (ulong)stream.Position;

            return true;
        }

        /// <summary>
        /// Writes the header information to a stream.
        /// </summary>
        /// <param name="stream">The stream to which the header data will be written.</param>
        public readonly void Write(Stream stream)
        {
            // Write various properties to the stream
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteInt32((int)ModelFlags, Endianness);
            stream.WriteUInt64(MeshCount, Endianness);
        }
    }
}