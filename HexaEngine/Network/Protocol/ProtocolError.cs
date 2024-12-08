namespace HexaEngine.Network.Protocol
{
    using Hexa.Protobuf;

    [ProtobufRecord]
    public partial struct ProtocolError : IRecord
    {
        public ErrorCode ErrorCode;
        public ErrorSeverity Severity;

        public ProtocolError(ErrorCode errorCode, ErrorSeverity severity)
        {
            ErrorCode = errorCode;
            Severity = severity;
        }

        public readonly RecordType Type => RecordType.ProtocolError;

        public override readonly string ToString()
        {
            return $"{Severity}: {ErrorCode}";
        }
    }
}