namespace HexaEngine.Network.Protocol
{
    using Hexa.Protobuf;
    using System;

    [ProtobufRecord]
    public partial struct RateLimit : IRecord
    {
        private long timestamp;
        private long rateLimitReset;
        public byte Warning;

        public RateLimit(DateTime timestamp, DateTime rateLimitReset, byte warning)
        {
            this.timestamp = timestamp.Ticks;
            this.rateLimitReset = rateLimitReset.Ticks;
            Warning = warning;
        }

        public readonly DateTime Timestamp => new(timestamp, DateTimeKind.Utc);

        public readonly DateTime RateLimitReset => new(rateLimitReset, DateTimeKind.Utc);

        public readonly RecordType Type => RecordType.RateLimit;
    }
}