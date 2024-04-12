namespace HexaEngine.Resources.Factories
{
    using System.Threading.Tasks;

    public class MeshResourceFactory : ResourceFactory<Mesh, MeshDesc>
    {
        public MeshResourceFactory(ResourceManager resourceManager) : base(resourceManager)
        {
        }

        protected override Mesh CreateInstance(ResourceManager manager, ResourceGuid name, MeshDesc desc)
        {
            return new(this, name, desc);
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