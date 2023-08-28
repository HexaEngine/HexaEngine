namespace HexaEngine.Core.IO.Shaders
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct ShaderSourceHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x00 };
        public static readonly Version Version = 2;
        public static readonly Version MinVersion = 2;

        public Endianness Endianness;
        public Encoding Encoding;
        public Compression Compression;
        public uint BytecodeLength;
        public uint InputElementCount;
        public uint MacroCount;
        public ShaderFlags Flags;
        public long ContentStart;

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

        public void Write(Stream stream)
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