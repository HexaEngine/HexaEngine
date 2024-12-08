namespace HexaEngine.Resources.Factories
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using System.Threading.Tasks;

    public class MaterialTextureResourceFactory : ResourceFactory<MaterialTexture, Core.IO.Binary.Materials.MaterialTexture>
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(Resources));
        private readonly IGraphicsDevice device;

        public MaterialTextureResourceFactory(ResourceManager resourceManager, IGraphicsDevice device) : base(resourceManager)
        {
            this.device = device;
        }

        protected override MaterialTexture CreateInstance(ResourceManager manager, ResourceGuid id, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            var artifact = ArtifactDatabase.GetArtifact(desc.File);

            if (artifact == null)
            {
                return null!;
            }

            return new(this, id, desc);
        }

        protected override void LoadInstance(ResourceManager manager, MaterialTexture instance, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            if (instance == null)
            {
                return;
            }

            var artifact = ArtifactDatabase.GetArtifact(desc.File);

            if (artifact == null)
            {
                Logger.Warn($"Failed to load texture, {desc.File}");
                return;
            }

            var tex = new Texture2D(new TextureFileDescription(artifact.Path));
            var sampler = device.CreateSamplerState(desc.GetSamplerDesc());
            instance.Initialize(tex, sampler);
        }

        protected override Task LoadInstanceAsync(ResourceManager manager, MaterialTexture instance, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            if (instance == null)
            {
                return Task.CompletedTask;
            }

            var artifact = ArtifactDatabase.GetArtifact(desc.File);

            if (artifact == null)
            {
                Logger.Warn($"Failed to load texture, {desc.File}");
                return Task.CompletedTask;
            }

            var tex = new Texture2D(new TextureFileDescription(artifact.Path));
            var sampler = device.CreateSamplerState(desc.GetSamplerDesc());
            instance.Initialize(tex, sampler);
            return Task.CompletedTask;
        }

        protected override void UnloadInstance(ResourceManager manager, MaterialTexture instance)
        {
        }
    }
}