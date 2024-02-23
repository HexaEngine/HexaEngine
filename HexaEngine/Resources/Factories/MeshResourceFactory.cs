namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Threading.Tasks;

    public class MeshResourceFactory : ResourceFactory<Mesh, MeshDesc>
    {
        private readonly IGraphicsDevice device;

        public MeshResourceFactory(ResourceManager resourceManager, IGraphicsDevice device) : base(resourceManager)
        {
            this.device = device;
        }

        protected override Mesh CreateInstance(ResourceManager manager, ResourceGuid name, MeshDesc desc)
        {
            return new(this, name, device, desc);
        }

        protected override void LoadInstance(ResourceManager manager, Mesh instance, MeshDesc desc)
        {
        }

        protected override Task LoadInstanceAsync(ResourceManager manager, Mesh instance, MeshDesc desc)
        {
            return Task.CompletedTask;
        }

        protected override void UnloadInstance(ResourceManager manager, Mesh instance)
        {
        }
    }
}