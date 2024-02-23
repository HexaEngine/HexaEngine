namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents the header information of a terrain file, including details such as endianness, encoding, compression, and material library.
    /// </summary>
    public struct TerrainHeader
    {
        /// <summary>
        /// The magic number used to identify terrain files.
        /// </summary>
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x00 };

        /// <summary>
        /// The current version of the terrain file format.
        /// </summary>
        public static readonly Version Version = 1;

        /// <summary>
        /// The minimum supported version of the terrain file format.
        /// </summary>
        public static readonly Version MinVersion = 1;

        /// <summary>
        /// The endianness of the terrain file.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The encoding used for text data in the terrain file.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// The compression method used for the terrain file.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// The number of layers in the terrain.
        /// </summary>
        public int Layers;

        /// <summary>
        /// The number of layer groups in the terrain.
        /// </summary>
        public int LayerGroups;

        /// <summary>
        /// The number of cells in the terrain.
        /// </summary>
        public int Cells;

        /// <summary>
        /// The position in the file where the content starts.
        /// </summary>
        public ulong ContentStart;

        /// <summary>
        /// Reads the header information from the specified stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the header information.</param>
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
            Layers = stream.ReadInt32(Endianness);
            LayerGroups = stream.ReadInt32(Endianness);
            Cells = stream.ReadInt32(Endianness);
            ContentStart = (ulong)stream.Position;
        }

        /// <summary>
        /// Writes the header information to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to which the header information will be written.</param>
        public readonly void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteInt32(Layers, Endianness);
            stream.WriteInt32(LayerGroups, Endianness);
            stream.WriteInt32(Cells, Endianness);
        }
    }
}