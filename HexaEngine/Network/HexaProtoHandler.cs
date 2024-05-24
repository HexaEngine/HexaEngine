namespace HexaEngine.Network
{
    using HexaEngine.Network.Events;
    using HexaEngine.Network.Protocol;
    using System.Net.Sockets;

    public class HexaProtoHandler : HexaProtoClientBase
    {
        private TimeSpan roundTripTime;
        private TimeSpan timeOffset;
        private TimeSpan clientLocalTimeOffset;

        private bool ready = false;
        private int requestCount;
        private DateTime rateLimitResetTimestamp;
        private byte rateLimitWarning;
        private bool heartbeatSend = false;

        public HexaProtoHandler(Socket socket) : base(socket)
        {
        }

        public event HandlerDisconnectedEventHandler? Disconnected;

        public event HandlerReceivedEventHandler? Received;

        public event HandlerReadyEventHandler? Ready;

        public event HandlerRateLimitEventHandler? RateLimited;

        public event HandlerHeartbeatEventHandler? Heartbeat;

        public delegate void HandlerDisconnectedEventHandler(HexaProtoHandler handler);

        public delegate void HandlerReceivedEventHandler(HexaProtoHandler handler, Record record);

        public delegate void HandlerReadyEventHandler(HexaProtoHandler handler, ClientReady clientReady);

        public delegate void HandlerRateLimitEventHandler(HexaProtoHandler handler, RateLimitEventArgs args);

        public delegate void HandlerHeartbeatEventHandler(HexaProtoHandler handler, HeartbeatEventArgs args);

        public TimeSpan RoundTripTime => roundTripTime;

        public TimeSpan TimeOffset => timeOffset;

        public TimeSpan ClientLocalTimeOffset => clientLocalTimeOffset;

        protected override void OnSocketError(SocketError error)
        {
            Disconnect(false);
            base.OnSocketError(error);
        }

        protected override void OnProtocolError(ProtocolError error)
        {
            if (error.Severity == ErrorSeverity.Fatal)
            {
                Disconnect(false);
            }

            base.OnProtocolError(error);
        }

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this);
        }

        protected virtual void OnHeartbeat(HeartbeatEventArgs args)
        {
            Heartbeat?.Invoke(this, args);
        }

        protected virtual void OnRateLimit(RateLimitEventArgs args)
        {
            RateLimited?.Invoke(this, args);
        }

        protected virtual void OnReady(ClientReady ready)
        {
            Ready?.Invoke(this, ready);
        }

        public async ValueTask DisconnectAsync(bool reuseSocket, CancellationToken cancellationToken)
        {
            await socket.DisconnectAsync(reuseSocket, cancellationToken);
            OnDisconnected();
        }

        public void Disconnect(bool reuseSocket)
        {
            Send(new Disconnect());
            socket.Disconnect(reuseSocket);
            OnDisconnected();
        }

        protected override void Dispose(bool disposing)
        {
            Disconnect(false);
            base.Dispose(disposing);
        }

        public void SendHeartbeat()
        {
            heartbeatSend = true;
            Send(Protocol.Heartbeat.CreateInitial());
        }

        protected override void Process(Record record)
        {
            DateTime now = DateTime.UtcNow;

            if (record.Type == RecordType.Heartbeat)
            {
                ProcessHeartbeat(record.GetHeartbeat(), now);
                return;
            }

            if (now >= rateLimitResetTimestamp)
            {
                Interlocked.Exchange(ref requestCount, 0);
                rateLimitResetTimestamp = now + TimeSpan.FromSeconds(1) - timeOffset;
                rateLimitWarning = 0;
            }

            int requestCountNow = Interlocked.Increment(ref requestCount);

            if (requestCountNow >= RateLimit)
            {
                if (rateLimitWarning >= 3)
                {
                    SendProtocolError(ErrorCode.RateLimit, ErrorSeverity.Fatal);
                }
                else
                {
                    rateLimitWarning++;
                    RateLimit rateLimit = new(now, rateLimitResetTimestamp + timeOffset, rateLimitWarning);
                    Send(rateLimit);
                    OnRateLimit(new(default, now, rateLimitResetTimestamp, rateLimitResetTimestamp, rateLimitWarning));
                }

                return;
            }

            if (ready)
            {
                Received?.Invoke(this, record);
            }

            switch (record.Type)
            {
                case RecordType.ProtocolError:
                    OnProtocolError(record.GetProtocolError());
                    break;

                case RecordType.ClientHello:
                    HandleClientHello(record);
                    break;

                case RecordType.ClientReady:
                    OnReady(record.GetClientReady());
                    break;

                case RecordType.Disconnect:
                    socket.Disconnect(false);
                    OnDisconnected();
                    break;

                case RecordType.TestPayload:
                    TestPayload payload = default;
                    payload.Read(record.AsSpan());
                    Send(payload);
                    break;
            }
        }

        private void ProcessHeartbeat(Heartbeat heartbeat, DateTime now)
        {
            if (!heartbeatSend)
            {
                SendProtocolError(ErrorCode.SequenceError, ErrorSeverity.Fatal);
                return;
            }

            if (heartbeat.Kind == HeartbeatKind.FirstTrip)
            {
                TimeSpan delay = now - heartbeat.Timestamp;
                TimeSpan rtt = heartbeat.PropagationDelay + delay;
                roundTripTime = rtt;
                timeOffset = rtt / 2;
                Send(Protocol.Heartbeat.CreateLast(roundTripTime));
                OnHeartbeat(new HeartbeatEventArgs(rtt, timeOffset));
                heartbeatSend = false;
            }
            else
            {
                SendProtocolError(ErrorCode.SequenceError, ErrorSeverity.Fatal);
                return;
            }
        }

        private void HandleClientHello(Record record)
        {
            if (ready)
            {
                SendProtocolError(ErrorCode.SequenceError, ErrorSeverity.Fatal);
                return;
            }
            ready = true;

            ClientHello clientHello = record.GetClientHello();
            clientLocalTimeOffset = clientHello.LocalTimeOffset;

            if (clientHello.GameVersion == 1)
            {
                Send(new ServerHello(1, DateTimeOffset.Now.Offset, HeartbeatRate, RateLimit, PayloadLimit));
            }
            else
            {
                SendProtocolError(ErrorCode.GameVersionMismatch, ErrorSeverity.Fatal);
            }
        }
    }
}