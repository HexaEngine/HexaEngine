namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using System.Threading.Tasks;

    public class MeshResourceFactory : ResourceFactory<Mesh, (MeshData, bool)>
    {
        private readonly IGraphicsDevice device;

        public MeshResourceFactory(IGraphicsDevice device)
        {
            this.device = device;
        }

        protected override Mesh CreateInstance(ResourceManager1 manager, string name, (MeshData, bool) instanceData)
        {
            return new(device, instanceData.Item1, instanceData.Item2);
        }

        protected override void LoadInstance(ResourceManager1 manager, Mesh instance, (MeshData, bool) instanceData)
        {
        }

        protected override Task LoadInstanceAsync(ResourceManager1 manager, Mesh instance, (MeshData, bool) instanceData)
        {
            return Task.CompletedTask;
        }

        protected override void UnloadInstance(ResourceManager1 manager, Mesh instance)
        {
        }
    }
}