namespace HexaEngine.Network.Protocol
{
    using System;
    using System.Buffers.Binary;

    public struct RateLimit : IRecord
    {
        public DateTime Timestamp;
        public DateTime RateLimitReset;
        public byte Warning;

        public RateLimit(DateTime timestamp, DateTime rateLimitReset, byte warning)
        {
            Timestamp = timestamp;
            RateLimitReset = rateLimitReset;
            Warning = warning;
        }

        public readonly RecordType Type => RecordType.RateLimit;

        public readonly int Write(Span<byte> span)
        {
            BinaryPrimitives.WriteInt64LittleEndian(span[0..], Timestamp.ToBinary());
            BinaryPrimitives.WriteInt64LittleEndian(span[8..], RateLimitReset.ToBinary());
            span[16] = Warning;
            return 17;
        }

        public int Read(ReadOnlySpan<byte> span)
        {
            Timestamp = DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(span[0..]));
            RateLimitReset = DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(span[8..]));
            Warning = span[16];
            return 17;
        }

        public readonly int SizeOf()
        {
            return 17;
        }

        public readonly void Free()
        {
        }
    }
}