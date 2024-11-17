namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public class MaterialTexture : ResourceInstance
    {
        public Texture2D Texture = null!;
        public IShaderResourceView ShaderResourceView = null!;
        public ISamplerState Sampler = null!;
        public Core.IO.Binary.Materials.MaterialTexture Desc;
        private volatile bool initialized = false;

        public MaterialTexture(IResourceFactory factory, ResourceGuid id, Core.IO.Binary.Materials.MaterialTexture desc) : base(factory, id)
        {
            Desc = desc;
        }

        public bool Initialized => initialized;

        public void Initialize(Texture2D texture, ISamplerState sampler)
        {
            Texture = texture;
            ShaderResourceView = texture.SRV!;
            Sampler = sampler;
            initialized = true;
        }

        protected override void ReleaseResources()
        {
            initialized = false;
            Texture.Dispose();
            ShaderResourceView?.Dispose();
            Sampler?.Dispose();
        }
    }
}