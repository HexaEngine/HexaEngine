namespace HexaEngine.Network
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Input;
    using HexaEngine.Network.Events;
    using HexaEngine.Network.Protocol;
    using System.Net;
    using System.Net.Sockets;

    public class Client : INetwork
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(Client));
        private readonly HexaProtoClient client;
        private readonly CancellationTokenSource receiveTaskCancellation = new();
        private Task? receiveTask;
        private bool running = true;

        private readonly PayloadBuffer payloadBuffer = new();
        private readonly object _lock = new();
        private readonly Queue<QueueRecord> records = [];

        public Client(IPEndPoint endPoint)
        {
            client = new(endPoint);
        }

        public bool IsClient => true;

        public bool IsServer => false;

        public int RetryAttempts { get; set; } = 3;

        public int RetryDelay { get; set; } = 1000;

        public event Action<HexaProtoClient>? Connected;

        public event ConnectionErrorEventHandler? ConnectionError;

        public delegate void ConnectionErrorEventHandler(HexaProtoClient sender, Exception exception, int retryAttempt);

        public event Action<HexaProtoClient, ServerHello>? Ready;

        public event Action<HexaProtoClient, bool>? Disconnected;

        public event Action<HexaProtoClient, Record>? Received;

        public bool IsConnected => client.IsConnected;

        public int ReceiveTimeout { get => client.ReceiveTimeout; set => client.ReceiveTimeout = value; }

        public int SendTimeout { get => client.SendTimeout; set => client.SendTimeout = value; }

        public EndPoint RemoteEndPoint => client.RemoteEndPoint;

        public EndPoint LocalEndPoint => client.LocalEndPoint;

        public TimeSpan RoundTripTime => client.RoundTripTime;

        public TimeSpan TimeOffset => client.TimeOffset;

        public TimeSpan ServerLocalTimeOffset => client.ServerLocalTimeOffset;

        public bool IsReady => client.IsReady;

        public async Task InitAsync()
        {
            SubscribeEvents(client);

            for (int i = 0; i < RetryAttempts; i++)
            {
                try
                {
                    await client.ConnectAsync(default);
                    Connected?.Invoke(client);
                    Logger.Info($"Connected to Server {client.RemoteEndPoint}");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to connect to server. Retry {i}");
                    Logger.Log(ex);
                    ConnectionError?.Invoke(client, ex, i);
                }

                await Task.Delay(RetryDelay);
            }
        }

        public void Init()
        {
            SubscribeEvents(client);

            for (int i = 0; i < RetryAttempts; i++)
            {
                try
                {
                    client.Connect();
                    Connected?.Invoke(client);
                    Logger.Info($"Connected to Server {client.RemoteEndPoint}");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to connect to server. Retry {i}");
                    Logger.Log(ex);
                    ConnectionError?.Invoke(client, ex, i);
                }

                Thread.Sleep(RetryDelay);
            }
        }

        public void Disconnect()
        {
            receiveTaskCancellation.Cancel();
            receiveTask?.Wait();
            receiveTaskCancellation.TryReset();

            client.Disconnect(true);
        }

        private void ClientConnected(HexaProtoClient client)
        {
            receiveTask = ReceiveVoid();
        }

        private void ClientReady(HexaProtoClient client, ServerHello serverHello)
        {
            Ready?.Invoke(client, serverHello);
        }

        private void ClientDisconnected(HexaProtoClient client, bool reuseSocket)
        {
            running = false;

            UnsubscribeEvents(client);

            Logger.Info("Disconnected");
            Disconnected?.Invoke(client, reuseSocket);
        }

        private void SubscribeEvents(HexaProtoClient client)
        {
            client.Connected += ClientConnected;
            client.Ready += ClientReady;
            client.Received += ClientReceived;
            client.Disconnected += ClientDisconnected;
            client.RateLimit += ClientRateLimit;
            client.Heartbeat += ClientHeartbeat;
            client.Error += ClientError;
            client.ProtocolError += ClientProtocolError;
        }

        private void UnsubscribeEvents(HexaProtoClient client)
        {
            client.Connected -= ClientConnected;
            client.Ready -= ClientReady;
            client.Received -= ClientReceived;
            client.Disconnected -= ClientDisconnected;
            client.RateLimit -= ClientRateLimit;
            client.Heartbeat -= ClientHeartbeat;
            client.Error -= ClientError;
            client.ProtocolError -= ClientProtocolError;
        }

        private void ClientProtocolError(HexaProtoClientBase sender, ProtocolErrorEventArgs error)
        {
            Logger.Error(error);
        }

        private void ClientError(SocketError socketError)
        {
            Logger.Error($"Socket error: {socketError}");
        }

        private void ClientHeartbeat(HexaProtoClient client, HeartbeatEventArgs args)
        {
            Logger.Info($"Heartbeat, UTC Time-Offset: {args.TimeOffset}, RTT: {args.RoundTripTime}");
        }

        private void ClientRateLimit(HexaProtoClient client, RateLimitEventArgs args)
        {
            Logger.Warn($"RateLimit, Reset: {args.LocalRateLimitReset}, Warn: {args.Warning}");
        }

        private void ClientReceived(HexaProtoClient client, Record record)
        {
            Received?.Invoke(client, record);
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

        private Task ReceiveVoid()
        {
            return Task.Factory.StartNew(async () =>
            {
                while (running)
                {
                    try
                    {
                        await client.ReceiveAsync(receiveTaskCancellation.Token);
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

        public void Tick()
        {
            while (DequeueRecord(out var record))
            {
                switch (record.Type)
                {
                    case RecordType.ProtocolError:
                        Logger.Info($"ProtocolError: {record.GetProtocolError()}");
                        break;

                    case RecordType.ClientHello:
                        break;

                    case RecordType.ServerHello:
                        break;

                    case RecordType.ClientReady:
                        break;

                    case RecordType.Disconnect:
                        break;

                    case RecordType.Heartbeat:
                        break;

                    case RecordType.RateLimit:
                        break;

                    case RecordType.Scene:
                        break;

                    case RecordType.Physics:
                        break;

                    case RecordType.ClientInput:
                        break;

                    case RecordType.User:
                        break;
                }

                ReturnRecord(record);
            }

            if (IsReady)
            {
                InputTick();

                ClientInput input = new();
                input.Axis = "FUCL";
                input.Value = 1;
                input.Flags = 0;
                client.Send(input);
                input.Free();
            }
        }

        private void InputTick()
        {
            InputManager inputManager = (InputManager)Input.Current;

            if (inputManager == null)
            {
                return;
            }

            ClientInput input = new();
            foreach (var axis in inputManager.VirtualAxes)
            {
                input.Axis = axis.Name;
                input.Value = axis.State.Value;
                input.Flags = axis.State.Flags;
                client.Send(input);
                input.Free();
            }
        }

        public void Send(IRecord record)
        {
            client.Send(record);
        }

        public void Send(Span<IRecord> records)
        {
            client.Send(records);
        }

        public ValueTask<bool> SendAsync(IRecord record, CancellationToken token)
        {
            return client.SendAsync(record, token);
        }

        public ValueTask<bool> SendAsync(Span<IRecord> records, CancellationToken token)
        {
            return client.SendAsync(records, token);
        }

        public void Dispose()
        {
            client.Dispose();
            running = false;
            receiveTaskCancellation.Cancel();
            receiveTask?.Wait();
            payloadBuffer.Dispose();

            UnsubscribeEvents(client);

            GC.SuppressFinalize(this);
        }
    }
}