namespace HexaEngine.Scenes.Systems
{
    using HexaEngine.Core;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;

    public class AudioSystem : ISceneSystem
    {
        private readonly ComponentTypeQuery<IAudioComponent> components = new();

        public string Name => "AudioSystem";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Destroy | SystemFlags.Update;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(components);
        }

        public void Destroy()
        {
            components.Dispose();
        }

        public void Update(float dt)
        {
            if (Application.InEditMode) return;

            for (int i = 0; i < components.Count; i++)
            {
                components[i].Update();
            }
        }
    }
}