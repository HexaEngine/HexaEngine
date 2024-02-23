namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;
    using System.Threading.Tasks;

    public class MaterialShaderResourceFactory : ResourceFactory<MaterialShader, MaterialShaderDesc>
    {
        private readonly IGraphicsDevice device;

        public MaterialShaderResourceFactory(ResourceManager resourceManager, IGraphicsDevice device) : base(resourceManager)
        {
            this.device = device;
        }

        protected override MaterialShader CreateInstance(ResourceManager manager, ResourceGuid id, MaterialShaderDesc instanceData)
        {
            return new MaterialShader(this, device, instanceData, id);
        }

        protected override void LoadInstance(ResourceManager manager, MaterialShader instance, MaterialShaderDesc instanceData)
        {
            instance.Initialize();
        }

        protected override Task LoadInstanceAsync(ResourceManager manager, MaterialShader instance, MaterialShaderDesc instanceData)
        {
            instance.Initialize();
            return Task.CompletedTask;
        }

        protected override void UnloadInstance(ResourceManager manager, MaterialShader instance)
        {
        }
    }
}