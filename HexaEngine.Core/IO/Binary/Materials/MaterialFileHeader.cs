namespace HexaEngine.Core.IO.Binary.Materials
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct MaterialFileHeader
    {
        /// <summary>
        /// Magic number used to identify the file format.
        /// </summary>
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x4D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x00];

        /// <summary>
        /// Current version of the material file format.
        /// </summary>
        public static readonly Version Version = 1;

        /// <summary>
        /// Minimum supported version of the material file format.
        /// </summary>
        public static readonly Version MinVersion = 1;

        /// <summary>
        /// The endianness of the material file.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The encoding used for strings in the material.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// Reads the material header from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the header from.</param>
        /// <exception cref="InvalidDataException">Thrown if the magic number is not found or if there is a version mismatch.</exception>
        public void Read(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                throw new InvalidDataException();
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out var version))
            {
                throw new InvalidDataException($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
        }

        /// <summary>
        /// Writes the material header to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write the header to.</param>
        public readonly void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
        }
    }
}