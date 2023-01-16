namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;

    public interface IResourceManager
    {
        public Core.Meshes.MeshData LoadMesh(string name);

        public Core.Meshes.MeshData CreateMesh(string name);

        public Material LoadMaterial(string name);

        public Material CreateMaterial(string name);

        public Core.Graphics.Texture LoadTexture(string name);

        public Core.Graphics.Texture CreateTexture(string name);

        public IEffect LoadEffect(string name);

        public IEffect CreateEffect(string name);
    }
}