namespace HexaEngine.Network.Protocol
{
    public enum RecordType : uint
    {
        ProtocolError,
        ClientHello,
        ServerHello,
        ClientReady,
        Disconnect,
        Heartbeat,
        RateLimit,
        Scene,
        Physics,
        ClientInput,
        User,
    }
}