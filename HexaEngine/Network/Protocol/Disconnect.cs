using Hexa.Protobuf;

namespace HexaEngine.Network.Protocol
{
    [ProtobufRecord]
    public partial struct Disconnect : IRecord
    {
        public readonly RecordType Type => RecordType.Disconnect;
    }
}