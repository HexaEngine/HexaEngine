namespace HexaEngine.Core.IO.Shaders
{
    using HexaEngine.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents the header information of a shader source file.
    /// </summary>
    public struct ShaderSourceHeader
    {
        /// <summary>
        /// Magic number used to identify the shader source file.
        /// </summary>
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x00];

        /// <summary>
        /// Current version of the shader source file format.
        /// </summary>
        public static readonly Version Version = 2;

        /// <summary>
        /// Minimum supported version of the shader source file format.
        /// </summary>
        public static readonly Version MinVersion = 2;

        /// <summary>
        /// The endianness of the shader source file.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The text encoding used in the shader source.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// The compression method used for the shader source.
        /// </summary>
        public Compression Compression;

        /// <summary>
        /// The length of the shader bytecode in the file.
        /// </summary>
        public uint BytecodeLength;

        /// <summary>
        /// The count of input elements used in the shader.
        /// </summary>
        public uint InputElementCount;

        /// <summary>
        /// The count of macros defined in the shader source.
        /// </summary>
        public uint MacroCount;

        /// <summary>
        /// Flags specifying additional information about the shader.
        /// </summary>
        public ShaderFlags Flags;

        /// <summary>
        /// The starting position of the shader source content within the file.
        /// </summary>
        public long ContentStart;

        /// <summary>
        /// Reads the shader source header from the given stream.
        /// </summary>
        /// <param name="stream">The input stream containing the shader source data.</param>
        /// <exception cref="InvalidDataException">Thrown when the magic number or version mismatch is detected.</exception>
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
            BytecodeLength = stream.ReadUInt32(Endianness);
            InputElementCount = stream.ReadUInt32(Endianness);
            MacroCount = stream.ReadUInt32(Endianness);
            Flags = (ShaderFlags)stream.ReadInt32(Endianness);
            ContentStart = stream.Position;
        }

        /// <summary>
        /// Tries to read the shader source header from the given stream.
        /// </summary>
        /// <param name="stream">The input stream containing the shader source data.</param>
        /// <returns><see langword="true"/> if the header was successfully read; otherwise, <see langword="false"/>.</returns>
        public bool TryRead(Stream stream)
        {
            if (!stream.Compare(MagicNumber))
            {
                return false;
            }

            Endianness = (Endianness)stream.ReadByte();
            if (!stream.CompareVersion(MinVersion, Version, Endianness, out _))
            {
                return false;
            }

            Encoding = Encoding.GetEncoding(stream.ReadInt32(Endianness));
            Compression = (Compression)stream.ReadInt32(Endianness);
            BytecodeLength = stream.ReadUInt32(Endianness);
            InputElementCount = stream.ReadUInt32(Endianness);
            MacroCount = stream.ReadUInt32(Endianness);
            Flags = (ShaderFlags)stream.ReadInt32(Endianness);
            ContentStart = stream.Position;
            return true;
        }

        /// <summary>
        /// Writes the shader source header to the specified stream.
        /// </summary>
        /// <param name="stream">The output stream to write the shader source header to.</param>
        public readonly void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteInt32((int)Compression, Endianness);
            stream.WriteUInt32(BytecodeLength, Endianness);
            stream.WriteUInt32(InputElementCount, Endianness);
            stream.WriteUInt32(MacroCount, Endianness);
            stream.WriteInt32((int)Flags, Endianness);
        }
    }
}