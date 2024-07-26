namespace HexaEngine.Network
{
    using HexaEngine.Core.Logging;
    using HexaEngine.Network.Events;
    using HexaEngine.Network.Protocol;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    public class ServerClientHandler : IDisposable
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ServerClientHandler));

        private readonly HexaProtoHandler handler;
        private readonly CancellationTokenSource receiveTaskCancellation = new();
        private Task? receiveTask;
        private bool running = true;

        private readonly PayloadBuffer payloadBuffer = new();
        private readonly object _lock = new();
        private readonly Queue<QueueRecord> records = [];

        private long lastHeartbeat;
        private long nextHeartbeat;

        public ServerClientHandler(HexaProtoHandler handler)
        {
            this.handler = handler;
            SubscribeEvents(handler);
        }

        private void SubscribeEvents(HexaProtoHandler handler)
        {
            handler.Disconnected += HandlerDisconnected;
            handler.Received += HandlerReceived;
            handler.Ready += HandlerReady;
            handler.RateLimit += HandlerRateLimit;
            handler.Heartbeat += HandlerHeartbeat;
            handler.Error += HandlerError;
            handler.ProtocolError += HandlerProtocolError;
        }

        private void UnsubscribeEvents(HexaProtoHandler handler)
        {
            handler.Disconnected -= HandlerDisconnected;
            handler.Received -= HandlerReceived;
            handler.Ready -= HandlerReady;
            handler.RateLimit -= HandlerRateLimit;
            handler.Heartbeat -= HandlerHeartbeat;
            handler.Error -= HandlerError;
            handler.ProtocolError -= HandlerProtocolError;
        }

        private void HandlerHeartbeat(HexaProtoHandler handler, HeartbeatEventArgs args)
        {
        }

        private void HandlerRateLimit(HexaProtoHandler handler, RateLimitEventArgs args)
        {
        }

        private void HandlerProtocolError(HexaProtoClientBase sender, ProtocolErrorEventArgs error)
        {
            Logger.Error(error);
        }

        private void HandlerError(SocketError socketError)
        {
            Logger.Error($"Socket error: {socketError}");
        }

        public bool Connected => handler.IsConnected;

        public int ReceiveTimeout { get => handler.ReceiveTimeout; set => handler.ReceiveTimeout = value; }

        public int SendTimeout { get => handler.SendTimeout; set => handler.SendTimeout = value; }

        public EndPoint RemoteEndPoint => handler.RemoteEndPoint;

        public EndPoint LocalEndPoint => handler.LocalEndPoint;

        public TimeSpan RoundTripTime => handler.RoundTripTime;

        public TimeSpan TimeOffset => handler.TimeOffset;

        public TimeSpan ClientLocalTimeOffset => handler.ClientLocalTimeOffset;

        public uint HeartbeatTimeout { get; set; } = 500;

        public bool IsReady => handler.IsReady;

        private void HandlerReady(HexaProtoHandler sender, ClientReady clientHello)
        {
            Ready?.Invoke(this, clientHello);
        }

        private void HandlerReceived(HexaProtoHandler sender, Record record)
        {
            Received?.Invoke(this, record);
            lock (_lock)
            {
                var segment = payloadBuffer.Rent((int)record.Length);
                if (segment == PayloadBufferSegment.Invalid)
                {
                    Logger.Warn("Payload Buffer Overflow, dropped record.");
                    return;
                }
                QueueRecord queueRecord = new(record.Type, segment);
                record.AsSpan().CopyTo(segment.AsSpan());
                records.Enqueue(queueRecord);
            }
        }

        public void Disconnect()
        {
            receiveTaskCancellation.Cancel();
            receiveTask?.Wait();
            receiveTaskCancellation.TryReset();

            handler.Disconnect(false);
        }

        private void HandlerDisconnected(HexaProtoHandler sender, bool reuseSocket)
        {
            running = false;

            UnsubscribeEvents(handler);

            Logger.Info("Disconnected");
            Disconnected?.Invoke(this);
        }

        public event Action<ServerClientHandler, ClientReady>? Ready;

        public event Action<ServerClientHandler>? Disconnected;

        public event Action<ServerClientHandler, Record>? Received;

        public void Init()
        {
            receiveTask = ReceiveVoid();
        }

        private Task ReceiveVoid()
        {
            return Task.Factory.StartNew(async () =>
            {
                while (running)
                {
                    try
                    {
                        await handler.ReceiveAsync(receiveTaskCancellation.Token);
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException)
                        {
                            return;
                        }
                        Logger.Log(ex);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Tick()
        {
            if (IsReady)
            {
                long now = Stopwatch.GetTimestamp();
                if (now >= nextHeartbeat)
                {
                    handler.SendHeartbeat();
                    ComputeNextHeartbeat(now);
                }
            }
        }

        private void ComputeNextHeartbeat(long now)
        {
            nextHeartbeat = now + (uint)(handler.HeartbeatRate / 1000f * Stopwatch.Frequency);
        }

        public bool DequeueRecord(out QueueRecord record)
        {
            lock (_lock)
            {
                return records.TryDequeue(out record);
            }
        }

        public void ReturnRecord(QueueRecord record)
        {
            lock (_lock)
            {
                payloadBuffer.Return(record.BufferSegment);
            }
        }

        public void Send(IRecord record)
        {
            handler.Send(record);
        }

        public void Send(Span<IRecord> records)
        {
            handler.Send(records);
        }

        public ValueTask<bool> SendAsync(IRecord record, CancellationToken token)
        {
            return handler.SendAsync(record, token);
        }

        public ValueTask<bool> SendAsync(Span<IRecord> records, CancellationToken token)
        {
            return handler.SendAsync(records, token);
        }

        public void Dispose()
        {
            handler.Dispose();
            running = false;
            receiveTaskCancellation.Cancel();
            receiveTask?.Wait();
            payloadBuffer.Dispose();

            UnsubscribeEvents(handler);

            GC.SuppressFinalize(this);
        }
    }
}