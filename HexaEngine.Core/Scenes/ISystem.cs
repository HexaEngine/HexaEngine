namespace HexaEngine.Core.Scenes
{
    using BepuUtilities;

    public interface ISystem
    {
        public string Name { get; }

        public void Register(GameObject gameObject);

        public void Unregister(GameObject gameObject);

        public void Awake(ThreadDispatcher dispatcher);

        public void Update(ThreadDispatcher dispatcher);

        public void FixedUpdate(ThreadDispatcher dispatcher);

        public void Destroy(ThreadDispatcher dispatcher);
    }
}