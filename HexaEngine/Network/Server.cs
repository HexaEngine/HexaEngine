namespace HexaEngine.Network
{
    using HexaEngine.Core.Logging;
    using HexaEngine.Network.Protocol;
    using System.Net;

    public class Server : INetwork
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(Server));
        private readonly HexaProtoServer server;
        private Task acceptTask;
        private bool running = true;
        private readonly List<ServerClientHandler> handlers = [];
        private readonly CancellationTokenSource acceptTaskCancellation = new();

        public Server(IPEndPoint endPoint)
        {
            server = new(endPoint);
        }

        public bool IsClient => false;

        public bool IsServer => true;

        public event Action<ServerClientHandler>? Accepted;

        public event Action<ServerClientHandler, ClientReady>? Ready;

        public event Action<ServerClientHandler>? Disconnected;

        public event Action<ServerClientHandler, Record>? Received;

        public int AcceptRateLimit { get; set; }

        public int AcceptBurstLimit { get; set; }

        public int AcceptBurstTimeWindow { get; set; }

        public Task InitAsync()
        {
            Init();
            return Task.CompletedTask;
        }

        public void Init()
        {
            server.Listen();
            acceptTask = AcceptVoid();
        }

        private Task AcceptVoid()
        {
            return Task.Factory.StartNew(async () =>
            {
                while (running)
                {
                    var handler = await server.AcceptAsync(acceptTaskCancellation.Token);

                    ServerClientHandler clientHandler = new(handler);
                    clientHandler.Disconnected += HandlerDisconnected;
                    clientHandler.Received += HandlerReceived;
                    clientHandler.Ready += HandlerReady;

                    handlers.Add(clientHandler);
                    Accepted?.Invoke(clientHandler);
                    clientHandler.Init();
                    Logger.Info($"Client Connected {clientHandler.RemoteEndPoint}");
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void HandlerReady(ServerClientHandler handler, ClientReady clientReady)
        {
            Ready?.Invoke(handler, clientReady);
        }

        private void HandlerReceived(ServerClientHandler handler, Record record)
        {
            Received?.Invoke(handler, record);
        }

        private void HandlerDisconnected(ServerClientHandler handler)
        {
            handlers.Remove(handler);
            Disconnected?.Invoke(handler);
            handler.Ready -= HandlerReady;
            handler.Disconnected -= HandlerDisconnected;
            handler.Received -= HandlerReceived;
        }

        public void Tick()
        {
            ReceiveTick();
        }

        private void ReceiveTick()
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];

                while (handler.DequeueRecord(out var record))
                {
                    if (record.Type != RecordType.ClientInput)
                    {
                        Logger.Info($"Received: {record.Type}");
                    }

                    handler.ReturnRecord(record);
                }

                handler.Tick();
            }
        }

        public void Broadcast(Span<IRecord> records)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].Send(records);
            }
        }

        public void Broadcast(IRecord record)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].Send(record);
            }
        }

        public void Multicast(Span<IRecord> records, Func<ServerClientHandler, bool> filter)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                if (filter(handler))
                {
                    handlers[i].Send(records);
                }
            }
        }

        public void Multicast(IRecord record, Func<ServerClientHandler, bool> filter)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                if (filter(handler))
                {
                    handlers[i].Send(record);
                }
            }
        }

        public void Dispose()
        {
            running = false;
            acceptTaskCancellation.Cancel();
            acceptTask.Wait();
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.Ready -= HandlerReady;
                handler.Disconnected -= HandlerDisconnected;
                handler.Received -= HandlerReceived;
                handler.Dispose();
            }
            handlers.Clear();
            server.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}