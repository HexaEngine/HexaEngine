namespace HexaEngine.Network
{
    public interface INetwork : IDisposable
    {
        public bool IsClient { get; }

        public bool IsServer { get; }

        public void Init();

        public Task InitAsync();

        public void Tick();
    }
}