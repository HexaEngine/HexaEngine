namespace HexaEngine.Objects
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;

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