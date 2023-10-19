namespace HexaEngine.Core.IO.Fonts
{
    using HexaEngine.Mathematics;

    public struct KerningPair
    {
        public char First;
        public char Second;
        public uint Amount;

        public static KerningPair Read(Stream stream, Endianness endianness)
        {
            KerningPair kerningPair;
            kerningPair.First = (char)stream.ReadUInt16(endianness);
            kerningPair.Second = (char)stream.ReadUInt16(endianness);
            kerningPair.Amount = stream.ReadUInt32(endianness);
            return kerningPair;
        }

        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteUInt16(First, endianness);
            stream.WriteUInt16(Second, endianness);
            stream.WriteUInt32(Amount, endianness);
        }
    }
}