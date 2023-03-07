namespace HexaEngine.Core.IO.Shaders
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct ShaderBytecodeHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6E, 0x73, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x00 };
        public Endianness Endianness;
        public const int Version = 2;
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
                throw new InvalidDataException("Magic number mismatch");
            Endianness = (Endianness)stream.ReadByte();
            if (!stream.Compare(Version, Endianness))
                throw new InvalidDataException("Version mismatch");

            Encoding = Encoding.GetEncoding(stream.ReadInt(Endianness));
            Compression = (Compression)stream.ReadInt(Endianness);
            BytecodeLength = stream.ReadUInt(Endianness);
            InputElementCount = stream.ReadUInt(Endianness);
            MacroCount = stream.ReadUInt(Endianness);
            Flags = (ShaderFlags)stream.ReadInt(Endianness);
            ContentStart = stream.Position;
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteInt(Encoding.CodePage, Endianness);
            stream.WriteInt((int)Compression, Endianness);
            stream.WriteUInt(BytecodeLength, Endianness);
            stream.WriteUInt(InputElementCount, Endianness);
            stream.WriteUInt(MacroCount, Endianness);
            stream.WriteInt((int)Flags, Endianness);
        }
    }
}