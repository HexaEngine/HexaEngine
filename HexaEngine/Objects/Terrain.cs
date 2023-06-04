namespace HexaEngine.Objects
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes.Components.Renderer;

    [EditorNode<Terrain>("Terrain")]
    public class Terrain : GameObject
    {
        public Terrain()
        {
            AddComponent(new TerrainRenderer());
        }

        [JsonConstructor]
        public Terrain(string name)
        {
            Name = name;
        }
    }
}