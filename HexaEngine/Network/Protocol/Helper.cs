namespace HexaEngine.Network.Protocol
{
    using System.Runtime.CompilerServices;

    public static class Helper
    {
        public static ClientHello GetClientHello(this Record record)
        {
            Unsafe.SkipInit(out ClientHello clientHello);
            clientHello.Read(record.AsSpan());
            return clientHello;
        }

        public static ServerHello GetServerHello(this Record record)
        {
            Unsafe.SkipInit(out ServerHello serverHello);
            serverHello.Read(record.AsSpan());
            return serverHello;
        }

        public static ClientReady GetClientReady(this Record record)
        {
            Unsafe.SkipInit(out ClientReady clientReady);
            clientReady.Read(record.AsSpan());
            return clientReady;
        }

        public static ClientInput GetClientInput(this Record record)
        {
            Unsafe.SkipInit(out ClientInput clientInput);
            clientInput.Read(record.AsSpan());
            return clientInput;
        }

        public static RateLimit GetRateLimit(this Record record)
        {
            Unsafe.SkipInit(out RateLimit rateLimit);
            rateLimit.Read(record.AsSpan());
            return rateLimit;
        }

        public static Heartbeat GetHeartbeat(this Record record)
        {
            Unsafe.SkipInit(out Heartbeat heartbeat);
            heartbeat.Read(record.AsSpan());
            return heartbeat;
        }

        public static ProtocolError GetProtocolError(this Record record)
        {
            Unsafe.SkipInit(out ProtocolError protocolError);
            protocolError.Read(record.AsSpan());
            return protocolError;
        }

        public static ClientHello GetClientHello(this QueueRecord record)
        {
            Unsafe.SkipInit(out ClientHello clientHello);
            clientHello.Read(record.AsSpan());
            return clientHello;
        }

        public static ServerHello GetServerHello(this QueueRecord record)
        {
            Unsafe.SkipInit(out ServerHello serverHello);
            serverHello.Read(record.AsSpan());
            return serverHello;
        }

        public static ClientReady GetClientReady(this QueueRecord record)
        {
            Unsafe.SkipInit(out ClientReady clientReady);
            clientReady.Read(record.AsSpan());
            return clientReady;
        }

        public static ClientInput GetClientInput(this QueueRecord record)
        {
            Unsafe.SkipInit(out ClientInput clientInput);
            clientInput.Read(record.AsSpan());
            return clientInput;
        }

        public static ProtocolError GetProtocolError(this QueueRecord record)
        {
            Unsafe.SkipInit(out ProtocolError protocolError);
            protocolError.Read(record.AsSpan());
            return protocolError;
        }
    }
}