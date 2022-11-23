namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;

    public interface IRecord
    {
        public int Encode(Span<byte> dest, Endianness endianness);

        public int Decode(Span<byte> src, Endianness endianness);

        public int Size();
    }
}