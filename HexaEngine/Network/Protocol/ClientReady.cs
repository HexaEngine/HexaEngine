namespace HexaEngine.Network.Protocol
{
    public struct ClientReady : IRecord
    {
        public readonly RecordType Type => RecordType.ClientReady;

        public readonly int Read(ReadOnlySpan<byte> span)
        {
            return 0;
        }

        public readonly int SizeOf()
        {
            return 0;
        }

        public readonly int Write(Span<byte> span)
        {
            return 0;
        }

        public readonly void Free()
        {
        }
    }
}