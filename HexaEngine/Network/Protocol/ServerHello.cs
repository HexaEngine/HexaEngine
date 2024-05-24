namespace HexaEngine.Network.Protocol
{
    using System;
    using System.Buffers.Binary;

    public struct ServerHello : IRecord
    {
        public ulong GameVersion;
        public TimeSpan LocalTimeOffset;
        public uint HeartbeatRate;
        public uint RateLimit;
        public uint PayloadLimit;

        public ServerHello(ulong gameVersion, TimeSpan localTimeOffset, uint heartbeatRate, uint rateLimit, uint payloadLimit)
        {
            GameVersion = gameVersion;
            LocalTimeOffset = localTimeOffset;
            HeartbeatRate = heartbeatRate;
            RateLimit = rateLimit;
            PayloadLimit = payloadLimit;
        }

        public readonly RecordType Type => RecordType.ServerHello;

        public readonly int Write(Span<byte> span)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(span[0..], GameVersion);
            BinaryPrimitives.WriteInt64LittleEndian(span[8..], LocalTimeOffset.Ticks);
            BinaryPrimitives.WriteUInt32LittleEndian(span[16..], HeartbeatRate);
            BinaryPrimitives.WriteUInt32LittleEndian(span[20..], RateLimit);
            BinaryPrimitives.WriteUInt32LittleEndian(span[24..], PayloadLimit);
            return 28;
        }

        public int Read(ReadOnlySpan<byte> span)
        {
            GameVersion = BinaryPrimitives.ReadUInt64LittleEndian(span[0..]);
            LocalTimeOffset = new(BinaryPrimitives.ReadInt64LittleEndian(span[8..]));
            HeartbeatRate = BinaryPrimitives.ReadUInt32LittleEndian(span[16..]);
            RateLimit = BinaryPrimitives.ReadUInt32LittleEndian(span[20..]);
            PayloadLimit = BinaryPrimitives.ReadUInt32LittleEndian(span[24..]);
            return 28;
        }

        public readonly int SizeOf()
        {
            return 28;
        }

        public void Free()
        {
        }
    }
}