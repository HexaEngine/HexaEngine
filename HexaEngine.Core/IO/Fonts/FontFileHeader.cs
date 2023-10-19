namespace HexaEngine.Core.IO.Fonts
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct FontFileHeader
    {
        public static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x46, 0x6F, 0x6E, 0x74, 0x00];
        public static readonly Version Version = 1;
        public static readonly Version MinVersion = 1;

        public Endianness Endianness;
        public Encoding Encoding;

        public string FontName;
        public uint FontSize;
        public uint BitmapWidth;
        public uint BitmapHeight;
        public uint PaddingUp;
        public uint PaddingDown;
        public uint PaddingLeft;
        public uint PaddingRight;
        public uint SpacingHorizontal;
        public uint SpacingVertical;
        public uint LineHeight;
        public uint Glyphs;
        public uint KerningPairs;

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
            FontSize = stream.ReadUInt32(Endianness);
            BitmapWidth = stream.ReadUInt32(Endianness);
            BitmapHeight = stream.ReadUInt32(Endianness);
            PaddingUp = stream.ReadUInt32(Endianness);
            PaddingDown = stream.ReadUInt32(Endianness);
            PaddingLeft = stream.ReadUInt32(Endianness);
            PaddingRight = stream.ReadUInt32(Endianness);
            SpacingHorizontal = stream.ReadUInt32(Endianness);
            SpacingVertical = stream.ReadUInt32(Endianness);
            LineHeight = stream.ReadUInt32(Endianness);
            Glyphs = stream.ReadUInt32(Endianness);
            KerningPairs = stream.ReadUInt32(Endianness);
            FontName = stream.ReadString(Encoding, Endianness);
        }

        public void Write(Stream stream)
        {
            stream.Write(MagicNumber);
            stream.WriteByte((byte)Endianness);
            stream.WriteUInt64(Version, Endianness);
            stream.WriteInt32(Encoding.CodePage, Endianness);
            stream.WriteUInt32(FontSize, Endianness);
            stream.WriteUInt32(BitmapWidth, Endianness);
            stream.WriteUInt32(BitmapHeight, Endianness);
            stream.WriteUInt32(PaddingUp, Endianness);
            stream.WriteUInt32(PaddingDown, Endianness);
            stream.WriteUInt32(PaddingLeft, Endianness);
            stream.WriteUInt32(PaddingRight, Endianness);
            stream.WriteUInt32(SpacingHorizontal, Endianness);
            stream.WriteUInt32(SpacingVertical, Endianness);
            stream.WriteUInt32(LineHeight, Endianness);
            stream.WriteUInt32(Glyphs, Endianness);
            stream.WriteUInt32(KerningPairs, Endianness);
            stream.WriteString(FontName, Encoding, Endianness);
        }
    }
}