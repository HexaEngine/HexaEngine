namespace HexaEngine.Objects
{
    using HexaEngine.Components.Physics.Collider;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;

    [EditorGameObject<Terrain>("Terrain")]
    public class Terrain : GameObject
    {
        public Terrain()
        {
            AddComponent(new TerrainRendererComponent());
            AddComponent(new TerrainCollider());
        }

        [JsonConstructor]
        public Terrain(string name)
        {
            Name = name;
        }
    }
}