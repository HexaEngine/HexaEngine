namespace HexaEngine.Network
{
    using HexaEngine.Network.Events;
    using HexaEngine.Network.Protocol;
    using System.Net;
    using System.Net.Sockets;

    public class HexaProtoClient : HexaProtoClientBase
    {
        private readonly IPEndPoint address;

        private TimeSpan roundTripTime;
        private TimeSpan timeOffset;
        private TimeSpan serverLocalTimeOffset;

        private bool ready = false;
        private DateTime rateLimitResetTimestamp;
        private volatile bool rateLimited = false;

        public HexaProtoClient(IPEndPoint address) : base(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            this.address = address;
        }

        public bool IsReady => ready;

        public TimeSpan RoundTripTime => roundTripTime;

        public TimeSpan TimeOffset => timeOffset;

        public TimeSpan ServerLocalTimeOffset => serverLocalTimeOffset;

        public event ClientConnectedEventHandler? Connected;

        public event ClientDisconnectedEventHandler? Disconnected;

        public event ClientReceivedEventHandler? Received;

        public event ClientReadyEventHandler? Ready;

        public event ClientRateLimitEventHandler? RateLimit;

        public event ClientHeartbeatEventHandler? Heartbeat;

        public delegate void ClientConnectedEventHandler(HexaProtoClient client);

        public delegate void ClientDisconnectedEventHandler(HexaProtoClient client, bool reuseSocket);

        public delegate void ClientReceivedEventHandler(HexaProtoClient client, Record record);

        public delegate void ClientReadyEventHandler(HexaProtoClient client, ServerHello serverHello);

        public delegate void ClientRateLimitEventHandler(HexaProtoClient client, RateLimitEventArgs args);

        public delegate void ClientHeartbeatEventHandler(HexaProtoClient client, HeartbeatEventArgs args);

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

        protected virtual void OnRateLimit(RateLimitEventArgs args)
        {
            RateLimit?.Invoke(this, args);
        }

        protected virtual void OnHeartbeat(HeartbeatEventArgs args)
        {
            Heartbeat?.Invoke(this, args);
        }

        protected virtual void OnReady(ServerHello serverHello)
        {
            Ready?.Invoke(this, serverHello);
        }

        public void Connect()
        {
            socket.Connect(address);

            CreateNetworkStream();

            Connected?.Invoke(this);

            ClientHello clientHello = new(1, DateTimeOffset.Now.Offset);
            Send(clientHello);
        }

        public async ValueTask ConnectAsync(CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(address, cancellationToken);

            CreateNetworkStream();

            Connected?.Invoke(this);

            ClientHello clientHello = new(1, DateTimeOffset.Now.Offset);
            Send(clientHello);
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

        protected override void Process(Record record)
        {
            DateTime now = DateTime.UtcNow;
            if (ready)
            {
                Received?.Invoke(this, record);
            }
            switch (record.Type)
            {
                case RecordType.ProtocolError:
                    OnProtocolError(record.GetProtocolError());
                    break;

                case RecordType.ServerHello:
                    HandleServerHello(record);
                    break;

                case RecordType.Disconnect:
                    Disconnect(false);
                    break;

                case RecordType.RateLimit:
                    HandleRateLimit(record.GetRateLimit(), now);
                    break;

                case RecordType.Heartbeat:
                    HandleHeartbeat(record.GetHeartbeat(), now);
                    break;
            }
        }

        private void HandleHeartbeat(Heartbeat heartbeat, DateTime now)
        {
            switch (heartbeat.Kind)
            {
                case HeartbeatKind.Initial:
                    TimeSpan delay = now - heartbeat.Timestamp;
                    Send(Protocol.Heartbeat.CreateFirstTrip(now, delay));
                    break;

                case HeartbeatKind.FirstTrip:
                    SendProtocolError(ErrorCode.SequenceError, ErrorSeverity.Fatal);
                    break;

                case HeartbeatKind.LastTrip:
                    HeartbeatEventArgs eventArgs = new(heartbeat);
                    roundTripTime = heartbeat.RoundTripTime;
                    timeOffset = eventArgs.TimeOffset;
                    OnHeartbeat(eventArgs);
                    break;
            }
        }

        private void HandleRateLimit(RateLimit rateLimit, DateTime now)
        {
            RateLimitEventArgs rateLimitEventArgs = new(this, rateLimit, now);
            rateLimitResetTimestamp = rateLimitEventArgs.LocalRateLimitReset;
            rateLimited = true;
            OnRateLimit(rateLimitEventArgs);
        }

        public override void Send<T>(T record)
        {
            LimitRate();
            base.Send(record);
        }

        public override void Send(Span<IRecord> records)
        {
            LimitRate();
            base.Send(records);
        }

        public override async ValueTask<bool> SendAsync(IRecord record, CancellationToken token = default)
        {
            await LimitRateAsync();
            return await base.SendAsync(record, token);
        }

        public override ValueTask<bool> SendAsync(Span<IRecord> records, CancellationToken token = default)
        {
            LimitRate();
            return base.SendAsync(records, token);
        }

        public void LimitRate()
        {
            if (rateLimited)
            {
                int delay = (int)Math.Ceiling((rateLimitResetTimestamp - DateTime.UtcNow).TotalMilliseconds);
                Thread.Sleep(delay);
                rateLimited = false;
            }
        }

        public async ValueTask LimitRateAsync()
        {
            if (rateLimited)
            {
                int delay = (int)Math.Ceiling((rateLimitResetTimestamp - DateTime.UtcNow).TotalMilliseconds);
                await Task.Delay(delay);
                rateLimited = false;
            }
        }

        protected virtual void HandleServerHello(Record record)
        {
            if (ready)
            {
                SendProtocolError(ErrorCode.SequenceError, ErrorSeverity.Fatal);
            }

            ready = true;

            ServerHello serverHello = record.GetServerHello();
            serverLocalTimeOffset = serverHello.LocalTimeOffset;
            RequestRateLimit = serverHello.RateLimit;
            PayloadLimit = serverHello.PayloadLimit;
            HeartbeatRate = serverHello.HeartbeatRate;

            if (serverHello.GameVersion == 1)
            {
                OnReady(serverHello);
                Send(new ClientReady());
            }
            else
            {
                SendProtocolError(ErrorCode.GameVersionMismatch, ErrorSeverity.Fatal);
            }
        }
    }
}