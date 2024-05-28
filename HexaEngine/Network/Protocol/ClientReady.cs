namespace HexaEngine.Network.Protocol
{
    using Hexa.Protobuf;

    [ProtobufRecord]
    public partial struct ClientReady : IRecord
    {
        public readonly RecordType Type => RecordType.ClientReady;
    }
}