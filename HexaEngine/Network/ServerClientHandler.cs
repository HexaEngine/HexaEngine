namespace HexaEngine.Network
{
    using HexaEngine.Network.Protocol;
    using System.Diagnostics;
    using System.Net;

    public unsafe struct QueueRecord
    {
        public RecordType Type;
        public PayloadBufferSegment BufferSegment;

        public QueueRecord(RecordType type, PayloadBufferSegment bufferSegment)
        {
            Type = type;
            BufferSegment = bufferSegment;
        }

        public readonly Span<byte> AsSpan()
        {
            return BufferSegment.AsSpan();
        }
    }

    public class ServerClientHandler : IDisposable
    {
        private readonly HexaProtoHandler handler;
        private readonly CancellationTokenSource receiveTaskCancellation = new();
        private Task? receiveTask;
        private bool running = true;
        private bool ready = false;

        private readonly PayloadBuffer payloadBuffer = new();
        private readonly object _lock = new();
        private readonly Queue<QueueRecord> records = [];

        private long lastHeartbeat;
        private long nextHeartbeat;

        public ServerClientHandler(HexaProtoHandler handler)
        {
            this.handler = handler;
            handler.Blocking = true;
            handler.Disconnected += HandlerDisconnected;
            handler.Received += HandlerReceived;
            handler.Ready += HandlerReady;
        }

        public bool Connected => handler.IsConnected;

        public int ReceiveTimeout { get => handler.ReceiveTimeout; set => handler.ReceiveTimeout = value; }

        public int SendTimeout { get => handler.SendTimeout; set => handler.SendTimeout = value; }

        public bool Blocking { get => handler.Blocking; set => handler.Blocking = value; }

        public EndPoint RemoteEndPoint => handler.RemoteEndPoint;

        public EndPoint LocalEndPoint => handler.LocalEndPoint;

        public TimeSpan RoundTripTime => handler.RoundTripTime;

        public TimeSpan TimeOffset => handler.TimeOffset;

        public TimeSpan ClientLocalTimeOffset => handler.ClientLocalTimeOffset;

        public uint HeartbeatTimeout { get; set; } = 500;

        public bool IsReady => ready;

        private void HandlerReady(HexaProtoHandler sender, ClientReady clientHello)
        {
            Ready?.Invoke(this, clientHello);
            ready = true;
        }

        private void HandlerReceived(HexaProtoHandler sender, Record record)
        {
            Received?.Invoke(this, record);
            lock (_lock)
            {
                var segment = payloadBuffer.Rent((int)record.Length);
                if (segment == PayloadBufferSegment.Invalid)
                {
                    Console.WriteLine("Payload Buffer Overflow, dropped record.");
                    return;
                }
                QueueRecord queueRecord = new(record.Type, segment);
                record.AsSpan().CopyTo(segment.AsSpan());
                records.Enqueue(queueRecord);
            }
        }

        private void HandlerDisconnected(HexaProtoHandler sender)
        {
            Disconnected?.Invoke(this);
            sender.Ready -= HandlerReady;
            sender.Disconnected -= HandlerDisconnected;
            sender.Received -= HandlerReceived;
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
                        Console.WriteLine(ex);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Tick()
        {
            if (ready)
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

            GC.SuppressFinalize(this);
        }
    }
}