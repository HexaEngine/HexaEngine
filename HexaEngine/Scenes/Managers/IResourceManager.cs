namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Graphics;
    using HexaEngine.Objects;
    using HexaEngine.Resources;

    public interface IResourceManager
    {
        public MeshData LoadMesh(string name);

        public MeshData CreateMesh(string name);

        public Material LoadMaterial(string name);

        public Material CreateMaterial(string name);

        public Graphics.Texture LoadTexture(string name);

        public Graphics.Texture CreateTexture(string name);

        public IEffect LoadEffect(string name);

        public IEffect CreateEffect(string name);
    }
}