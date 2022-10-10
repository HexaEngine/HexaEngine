namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;

    public interface IRecord
    {
        public void Encode(Span<byte> dest, Endianness endianness);

        public void Decode(Span<byte> src, Endianness endianness);

        public int Size();
    }
}