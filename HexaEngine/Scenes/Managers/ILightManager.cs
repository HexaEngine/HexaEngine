namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Lights;
    using System.Collections.Generic;

    public interface ILightManager
    {
        public IReadOnlyList<Light> Lights { get; }

        public RenderQueue Queue { get; set; }

        public void AddLight(Light light);

        public void RemoveLight(Light light);

        public void Update();

        public void Draw();
    }
}