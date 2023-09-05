namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Resources;

    public static class MeshResourceFactoryExtensions
    {
        public static Mesh LoadMesh(this ResourceManager1 manager, MeshData data, bool debone = false)
        {
            return manager.CreateInstance<Mesh, (MeshData, bool)>(data.Name, (data, debone)) ?? throw new NotSupportedException("The factory was not found");
        }

        public static async Task<Mesh> LoadMeshAsync(this ResourceManager1 manager, MeshData mesh, bool debone = false)
        {
            return await Task.Factory.StartNew(() => manager.LoadMesh(mesh, debone));
        }

        public static void UnloadMesh(this ResourceManager1 manager, Mesh mesh)
        {
            manager.DestroyInstance(mesh);
        }
    }
}