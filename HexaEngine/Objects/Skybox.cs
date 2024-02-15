namespace HexaEngine.Objects
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;

    [EditorGameObject<Skybox>("Skybox")]
    public class Skybox : GameObject
    {
        public Skybox()
        {
            AddComponent(new SkyRendererComponent());
        }

        [JsonConstructor]
        public Skybox(string name)
        {
            Name = name;
        }
    }
}