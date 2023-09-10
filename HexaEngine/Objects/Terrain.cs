namespace HexaEngine.Objects
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;

    [EditorGameObject<Terrain>("TerrainCellData")]
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