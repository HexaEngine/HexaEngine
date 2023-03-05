namespace HexaEngine.Core.Scenes.Systems
{
    using BepuUtilities;
    using System.Collections.Generic;

    public class AudioSystem : ISystem
    {
        private readonly List<IAudioComponent> components = new();

        public string Name => "AudioSystem";

        public void Awake(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Update();
            }
        }

        public void FixedUpdate(ThreadDispatcher dispatcher)
        {
        }

        public void Register(GameObject gameObject)
        {
            components.AddComponentIfIs(gameObject);
        }

        public void Unregister(GameObject gameObject)
        {
            components.RemoveComponentIfIs(gameObject);
        }
    }
}