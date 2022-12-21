namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Graphics;
    using HexaEngine.Objects;

    public interface IResourceManager
    {
        public Mesh LoadMesh(string name);

        public Mesh CreateMesh(string name);

        public Material LoadMaterial(string name);

        public Material CreateMaterial(string name);

        public Texture LoadTexture(string name);

        public Texture CreateTexture(string name);

        public Effect LoadEffect(string name);

        public Effect CreateEffect(string name);
    }
}