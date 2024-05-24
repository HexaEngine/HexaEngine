namespace HexaEngine.Network
{
    using HexaEngine.Core.Debugging;
    using System.Net;
    using System.Net.Sockets;

    public class BaseSocket : IDisposable
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(BaseSocket));

        protected readonly Socket socket;
        private NetworkStream stream;

        private bool disposedValue;

        public unsafe BaseSocket(AddressFamily family, SocketType type, ProtocolType protocol)
        {
            socket = new(family, type, protocol);

            socket.NoDelay = true;
            socket.Blocking = true;
        }

        public unsafe BaseSocket(Socket socket)
        {
            this.socket = socket;

            socket.NoDelay = true;
            socket.Blocking = true;

            stream = new NetworkStream(socket);
        }

        public bool IsConnected => socket.Connected;

        public int ReceiveTimeout { get => socket.ReceiveTimeout; set => socket.ReceiveTimeout = value; }

        public int SendTimeout { get => socket.SendTimeout; set => socket.SendTimeout = value; }

        public bool Blocking { get => socket.Blocking; set => socket.Blocking = value; }

        public EndPoint RemoteEndPoint => socket.RemoteEndPoint;

        public EndPoint LocalEndPoint => socket.LocalEndPoint;

        public event Action<SocketError>? Error;

        protected void CreateNetworkStream()
        {
            stream = new(socket);
        }

        protected virtual void OnSocketError(SocketError error)
        {
            Error?.Invoke(error);
        }

        protected unsafe int Receive(Memory<byte> memory)
        {
            int receivedBytes = stream.Read(memory.Span);

            if (receivedBytes == 0)
            {
                OnSocketError(SocketError.SocketError);
                return 0;
            }

            return receivedBytes;
        }

        protected async ValueTask<int> ReceiveAsync(Memory<byte> memory, CancellationToken token)
        {
            int receivedBytes = await stream.ReadAsync(memory, token);

            if (receivedBytes == 0)
            {
                OnSocketError(SocketError.SocketError);
                return 0;
            }

            return receivedBytes;
        }

        protected bool Send(Span<byte> data)
        {
            socket.Send(data, SocketFlags.None, out var error);
            if (error != SocketError.Success)
            {
                Logger.Log($"Socket send error: {error}");
                OnSocketError(error);
                return false;
            }
            return true;
        }

        protected async ValueTask<bool> SendAsync(Memory<byte> data, SocketFlags flags, CancellationToken token = default)
        {
            var task = socket.SendAsync(data, flags, token);
            await task;

            if (task.IsFaulted)
            {
                Logger.Log($"Socket async send error");
                OnSocketError(SocketError.SocketError);
                return false;
            }

            return true;
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                socket.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}