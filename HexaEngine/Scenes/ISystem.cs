using HexaEngine.Core.Scenes;

namespace HexaEngine.Scenes
{
    using HexaEngine.Collections;

    public interface ISystem : IHasFlags<SystemFlags>
    {
        public string Name { get; }

        public void Register(GameObject gameObject);

        public void Unregister(GameObject gameObject);

        public void Awake();

        public void Update(float delta);

        public void FixedUpdate();

        public void Destroy();
    }
}