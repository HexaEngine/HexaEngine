namespace HexaEngine.Network
{
    using HexaEngine.Core;
    using HexaEngine.Scenes;
    using System.Net;

    public class NetworkSystem : ISceneSystem
    {
        private readonly NetworkMode mode = NetworkMode.None;
        private INetwork? network;

        public string Name { get; } = "Network";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.EarlyUpdate | SystemFlags.Destroy;

        public void Awake(Scene scene)
        {
            if (Application.InEditMode)
            {
                return;
            }

            switch (mode)
            {
                case NetworkMode.Server:
                    network = new Server(new(IPAddress.Parse("127.0.0.1"), 28900));
                    break;

                case NetworkMode.Client:
                    network = new Client(new(IPAddress.Parse("127.0.0.1"), 28900));
                    break;

                case NetworkMode.ClientServer:
                    network = new ClientServer();
                    break;

                default:
                    break;
            }

            network?.Init();
        }

        public void Update(float delta)
        {
            if (Application.InEditMode)
            {
                return;
            }

            network?.Tick();
        }

        public void Destroy()
        {
            network?.Dispose();
        }
    }
}