namespace HexaEngine.Network.Protocol
{
    using Hexa.Protobuf;
    using System;

    [ProtobufRecord]
    public partial struct ServerHello : IRecord
    {
        public ulong GameVersion;
        private long localTimeOffset;
        public uint HeartbeatRate;
        public uint RateLimit;
        public uint PayloadLimit;

        public ServerHello(ulong gameVersion, TimeSpan localTimeOffset, uint heartbeatRate, uint rateLimit, uint payloadLimit)
        {
            GameVersion = gameVersion;
            this.localTimeOffset = localTimeOffset.Ticks;
            HeartbeatRate = heartbeatRate;
            RateLimit = rateLimit;
            PayloadLimit = payloadLimit;
        }

        public readonly RecordType Type => RecordType.ServerHello;

        public readonly TimeSpan LocalTimeOffset => new(localTimeOffset);
    }
}