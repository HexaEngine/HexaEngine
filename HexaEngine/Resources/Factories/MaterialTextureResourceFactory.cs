namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using System.Threading.Tasks;

    public class MaterialTextureResourceFactory : ResourceFactory<ResourceInstance<MaterialTexture>, Core.IO.Materials.MaterialTexture>
    {
        private readonly IGraphicsDevice device;

        public MaterialTextureResourceFactory(ResourceManager resourceManager, IGraphicsDevice device) : base(resourceManager)
        {
            this.device = device;
        }

        protected override ResourceInstance<MaterialTexture> CreateInstance(ResourceManager manager, string name, Core.IO.Materials.MaterialTexture desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (string.IsNullOrWhiteSpace(desc.File))
            {
                return null;
            }

            return new(this, fullname);
        }

        protected override void LoadInstance(ResourceManager manager, ResourceInstance<MaterialTexture> instance, Core.IO.Materials.MaterialTexture desc)
        {
            if (instance == null)
            {
                return;
            }
            string fullname = Paths.CurrentTexturePath + desc.File;
            var tex = new Texture2D(device, new TextureFileDescription(fullname));
            var sampler = device.CreateSamplerState(desc.GetSamplerDesc());
            instance.EndLoad(new(tex, sampler, desc));
        }

        protected override Task LoadInstanceAsync(ResourceManager manager, ResourceInstance<MaterialTexture> instance, Core.IO.Materials.MaterialTexture desc)
        {
            if (instance == null)
            {
                return Task.CompletedTask;
            }
            string fullname = Paths.CurrentTexturePath + desc.File;
            var tex = new Texture2D(device, new TextureFileDescription(fullname));
            var sampler = device.CreateSamplerState(desc.GetSamplerDesc());
            instance.EndLoad(new(tex, sampler, desc));
            return Task.CompletedTask;
        }

        protected override void UnloadInstance(ResourceManager manager, ResourceInstance<MaterialTexture> instance)
        {
        }
    }
}