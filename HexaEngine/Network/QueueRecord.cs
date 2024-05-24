namespace HexaEngine.Network
{
    using HexaEngine.Network.Protocol;

    public unsafe struct QueueRecord
    {
        public RecordType Type;
        public PayloadBufferSegment BufferSegment;

        public QueueRecord(RecordType type, PayloadBufferSegment bufferSegment)
        {
            Type = type;
            BufferSegment = bufferSegment;
        }

        public readonly Span<byte> AsSpan()
        {
            return BufferSegment.AsSpan();
        }
    }
}