namespace HexaEngine.Network.Protocol
{
    using Hexa.Protobuf;
    using System;

    [ProtobufRecord]
    public partial struct ClientHello : IRecord
    {
        public ulong GameVersion;
        public long LocalTimeOffsetTicks;

        public readonly TimeSpan LocalTimeOffset => new(LocalTimeOffsetTicks);

        public ClientHello(ulong gameVersion, TimeSpan localTimeOffset)
        {
            GameVersion = gameVersion;
            LocalTimeOffsetTicks = localTimeOffset.Ticks;
        }

        public readonly RecordType Type => RecordType.ClientHello;
    }
}