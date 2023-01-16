namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Meshes;
    using HexaEngine.Graphics;
    using HexaEngine.IO;
    using HexaEngine.Resources;

    public interface IResourceManager
    {
        public Core.Meshes.MeshData LoadMesh(string name);

        public Core.Meshes.MeshData CreateMesh(string name);

        public Material LoadMaterial(string name);

        public Material CreateMaterial(string name);

        public Graphics.Texture LoadTexture(string name);

        public Graphics.Texture CreateTexture(string name);

        public IEffect LoadEffect(string name);

        public IEffect CreateEffect(string name);
    }
}