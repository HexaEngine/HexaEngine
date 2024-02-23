namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Resources;

    public static class MeshResourceFactoryExtensions
    {
        public static Mesh LoadMesh(this ResourceManager manager, IMeshData data, ILODData lodData)
        {
            return LoadMesh(manager, new MeshDesc(data, lodData));
        }

        public static Task<Mesh> LoadMeshAsync(this ResourceManager manager, IMeshData mesh, ILODData lodData)
        {
            return LoadMeshAsync(manager, new MeshDesc(mesh, lodData));
        }

        public static Mesh LoadMesh(this ResourceManager manager, MeshDesc desc)
        {
            return manager.CreateInstance<Mesh, MeshDesc>(new(desc.MeshData.Guid, (int)desc.LODData.LODLevel), desc) ?? throw new NotSupportedException("The factory was not found");
        }

        public static async Task<Mesh> LoadMeshAsync(this ResourceManager manager, MeshDesc desc)
        {
            return await Task.Factory.StartNew(() => manager.LoadMesh(desc));
        }
    }
}