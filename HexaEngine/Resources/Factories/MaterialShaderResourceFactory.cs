namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using System.Threading.Tasks;

    public class MaterialShaderResourceFactory : ResourceFactory<ResourceInstance<Resources.MaterialShader>, (MeshData, MaterialData, bool)>
    {
        private readonly IGraphicsDevice device;

        public MaterialShaderResourceFactory(IGraphicsDevice device)
        {
            this.device = device;
        }

        protected override ResourceInstance<Resources.MaterialShader> CreateInstance(ResourceManager1 manager, string name, (MeshData, MaterialData, bool) instanceData)
        {
            return new ResourceInstance<Resources.MaterialShader>(name, 0);
        }

        protected override void LoadInstance(ResourceManager1 manager, ResourceInstance<Resources.MaterialShader> instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData material, bool debone) = instanceData;
            var shader = new Resources.MaterialShader(device, mesh, material, debone);
            shader.Initialize();
            instance.EndLoad(shader);
        }

        protected override async Task LoadInstanceAsync(ResourceManager1 manager, ResourceInstance<Resources.MaterialShader> instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData material, bool debone) = instanceData;
            var shader = new Resources.MaterialShader(device, mesh, material, debone);
            await shader.InitializeAsync();
            instance.EndLoad(shader);
        }

        protected override void UnloadInstance(ResourceManager1 manager, ResourceInstance<Resources.MaterialShader> instance)
        {
        }
    }
}