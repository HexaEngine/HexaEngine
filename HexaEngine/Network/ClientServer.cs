namespace HexaEngine.Network
{
    using System.Net;
    using System.Threading.Tasks;

    public class ClientServer : INetwork
    {
        private Server server = null!;
        private Client client = null!;

        public bool IsClient { get; } = true;

        public bool IsServer { get; } = true;

        public async Task InitAsync()
        {
            server = new(new(IPAddress.Parse("127.0.0.1"), 28900));
            await server.InitAsync();

            client = new(new(IPAddress.Parse("127.0.0.1"), 28900));
            await client.InitAsync();
        }

        public void Init()
        {
            server = new(new(IPAddress.Parse("127.0.0.1"), 28900));
            server.Init();

            client = new(new(IPAddress.Parse("127.0.0.1"), 28900));
            client.Init();
        }

        public unsafe void Tick()
        {
            server.Tick();
            client.Tick();
        }

        public unsafe void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}