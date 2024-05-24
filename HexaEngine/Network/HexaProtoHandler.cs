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
        private int rateLimitWarningLimit = 3;
        private TimeSpan requestRateLimitTimeout = TimeSpan.FromSeconds(1);

        public HexaProtoHandler(Socket socket) : base(socket)
        {
        }

        public bool IsReady => ready;

        public TimeSpan RoundTripTime => roundTripTime;

        public TimeSpan TimeOffset => timeOffset;

        public TimeSpan ClientLocalTimeOffset => clientLocalTimeOffset;

        public int RateLimitWarningLimit { get => rateLimitWarningLimit; set => rateLimitWarningLimit = value; }

        public TimeSpan RequestRateLimitTimeout { get => requestRateLimitTimeout; set => requestRateLimitTimeout = value; }

        public event HandlerDisconnectedEventHandler? Disconnected;

        public event HandlerReceivedEventHandler? Received;

        public event HandlerReadyEventHandler? Ready;

        public event HandlerRateLimitEventHandler? RateLimit;

        public event HandlerHeartbeatEventHandler? Heartbeat;

        public delegate void HandlerDisconnectedEventHandler(HexaProtoHandler handler, bool reuseSocket);

        public delegate void HandlerReceivedEventHandler(HexaProtoHandler handler, Record record);

        public delegate void HandlerReadyEventHandler(HexaProtoHandler handler, ClientReady clientReady);

        public delegate void HandlerRateLimitEventHandler(HexaProtoHandler handler, RateLimitEventArgs args);

        public delegate void HandlerHeartbeatEventHandler(HexaProtoHandler handler, HeartbeatEventArgs args);

        protected override void OnSocketError(SocketError error)
        {
            Disconnect(true);
            base.OnSocketError(error);
        }

        protected override void OnProtocolError(ProtocolError error)
        {
            if (error.Severity == ErrorSeverity.Fatal)
            {
                Disconnect(true);
            }

            base.OnProtocolError(error);
        }

        protected virtual void OnDisconnected(bool reuseSocket)
        {
            Disconnected?.Invoke(this, reuseSocket);
        }

        protected virtual void OnHeartbeat(HeartbeatEventArgs args)
        {
            Heartbeat?.Invoke(this, args);
        }

        protected virtual void OnRateLimit(RateLimitEventArgs args)
        {
            RateLimit?.Invoke(this, args);
        }

        protected virtual void OnReady(ClientReady ready)
        {
            Ready?.Invoke(this, ready);
        }

        public void Disconnect(bool reuseSocket)
        {
            ready = false;
            socket.Disconnect(reuseSocket);
            OnDisconnected(reuseSocket);
        }

        public async ValueTask DisconnectAsync(bool reuseSocket, CancellationToken cancellationToken)
        {
            ready = false;
            await socket.DisconnectAsync(reuseSocket, cancellationToken);
            OnDisconnected(reuseSocket);
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

            switch (record.Type)
            {
                case RecordType.Heartbeat:
                    ProcessHeartbeat(record.GetHeartbeat(), now);
                    return;

                case RecordType.ProtocolError:
                    OnProtocolError(record.GetProtocolError());
                    return;
            }

            if (now >= rateLimitResetTimestamp)
            {
                Interlocked.Exchange(ref requestCount, 0);
                rateLimitResetTimestamp = now + requestRateLimitTimeout - timeOffset;
                rateLimitWarning = 0;
            }

            int requestCountNow = Interlocked.Increment(ref requestCount);

            if (requestCountNow >= RequestRateLimit)
            {
                if (rateLimitWarning >= RateLimitWarningLimit)
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
                case RecordType.ClientHello:
                    HandleClientHello(record);
                    break;

                case RecordType.ClientReady:
                    OnReady(record.GetClientReady());
                    break;

                case RecordType.Disconnect:
                    Disconnect(false);
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
                Send(new ServerHello(1, DateTimeOffset.Now.Offset, HeartbeatRate, RequestRateLimit, PayloadLimit));
            }
            else
            {
                SendProtocolError(ErrorCode.GameVersionMismatch, ErrorSeverity.Fatal);
            }
        }
    }
}