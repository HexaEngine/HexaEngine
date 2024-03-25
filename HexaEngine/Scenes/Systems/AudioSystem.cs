namespace HexaEngine.Scenes.Systems
{
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;

    public class AudioSystem : ISceneSystem
    {
        private readonly ComponentTypeQuery<IAudioComponent> components = new();

        public string Name => "AudioSystem";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Update;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(components);
        }

        public void Update(float dt)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Update();
            }
        }
    }
}