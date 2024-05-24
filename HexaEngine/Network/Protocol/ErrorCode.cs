namespace HexaEngine.Network.Protocol
{
    public enum ErrorCode : uint
    {
        None = 0,
        UnknownRecordType = 1,
        SequenceError = 2,
        ProtocolVersionMismatch = 3,
        PayloadTooLarge = 4,
        RateLimit = 5,
        HeartbeatTimeout = 6,
        GameVersionMismatch = 7,
    }
}