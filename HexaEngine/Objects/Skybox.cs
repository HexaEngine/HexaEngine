namespace HexaEngine.Objects
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Renderers.Components;
    using HexaEngine.Core.Scenes;

    [EditorNode<Skybox>("Skybox")]
    public class Skybox : GameObject
    {
        public Skybox()
        {
            AddComponent(new SkyboxRendererComponent());
        }

        [JsonConstructor]
        public Skybox(string name)
        {
            Name = name;
        }
    }
}