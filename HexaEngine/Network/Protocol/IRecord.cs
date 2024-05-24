namespace HexaEngine.Network.Protocol
{
    public interface IRecord
    {
        public RecordType Type { get; }

        public int Write(Span<byte> span);

        public int Read(ReadOnlySpan<byte> span);

        public int SizeOf();

        public void Free();
    }
}