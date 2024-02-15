namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Resources;

    public static class MeshResourceFactoryExtensions
    {
        public static Mesh LoadMesh(this ResourceManager manager, MeshData data, bool debone = false)
        {
            return manager.CreateInstance<Mesh, (MeshData, bool)>(data.Guid, (data, debone)) ?? throw new NotSupportedException("The factory was not found");
        }

        public static async Task<Mesh> LoadMeshAsync(this ResourceManager manager, MeshData mesh, bool debone = false)
        {
            return await Task.Factory.StartNew(() => manager.LoadMesh(mesh, debone));
        }
    }
}