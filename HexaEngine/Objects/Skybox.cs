namespace HexaEngine.Objects
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights.Probes;

    [EditorGameObject<Skybox>("Skybox")]
    public class Skybox : GameObject
    {
        public Skybox()
        {
            AddComponent(new SkyRendererComponent());
            AddComponent(new IBLLightProbeComponent());
        }

        [JsonConstructor]
        public Skybox(string name)
        {
            Name = name;
        }
    }
}