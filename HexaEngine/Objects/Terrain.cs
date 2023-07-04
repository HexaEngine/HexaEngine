namespace HexaEngine.Objects
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes.Components.Renderer;

    [EditorGameObject<Terrain>("Terrain")]
    public class Terrain : GameObject
    {
        public Terrain()
        {
            AddComponent(new TerrainRendererComponent());
        }

        [JsonConstructor]
        public Terrain(string name)
        {
            Name = name;
        }
    }
}