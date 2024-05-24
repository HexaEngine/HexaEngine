namespace HexaEngine.Network
{
    using System.Net;
    using System.Net.Sockets;

    public class HexaProtoServer : BaseSocket
    {
        private readonly IPEndPoint address;

        public HexaProtoServer(IPEndPoint address) : base(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            this.address = address;
            socket.Blocking = true;
        }

        public void Listen()
        {
            socket.Bind(address);
            socket.Listen();
        }

        public void Listen(int backlog)
        {
            socket.Listen(backlog);
        }

        public HexaProtoHandler Accept()
        {
            return new HexaProtoHandler(socket.Accept());
        }

        public async ValueTask<HexaProtoHandler> AcceptAsync(CancellationToken cancellationToken)
        {
            Socket client = await socket.AcceptAsync(cancellationToken);

            return new HexaProtoHandler(client);
        }
    }
}