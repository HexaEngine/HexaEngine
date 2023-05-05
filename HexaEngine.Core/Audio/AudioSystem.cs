using HexaEngine.Core.Scenes;

namespace HexaEngine.Core.Audio
{
    using System.Collections.Generic;

    public class AudioSystem : ISystem
    {
        private readonly List<IAudioComponent> components = new();

        public string Name => "AudioSystem";

        public SystemFlags Flags => SystemFlags.Update;

        public void Awake()
        {
        }

        public void Destroy()
        {
        }

        public void Update(float dt)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Update();
            }
        }

        public void FixedUpdate()
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