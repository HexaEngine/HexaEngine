namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public class MaterialTexture : IDisposable
    {
        public Texture2D Texture;
        public IShaderResourceView ShaderResourceView;
        public ISamplerState Sampler;
        public Core.IO.Binary.Materials.MaterialTexture Desc;
        private bool disposedValue;

        public MaterialTexture(Texture2D texture, ISamplerState sampler, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            Texture = texture;
            ShaderResourceView = texture.SRV;
            Sampler = sampler;
            Desc = desc;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Texture.Dispose();
                ShaderResourceView?.Dispose();
                Sampler?.Dispose();
                disposedValue = true;
            }
        }

        ~MaterialTexture()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}