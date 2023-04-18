namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;

    public class MaterialTexture : IDisposable
    {
        public IShaderResourceView ShaderResourceView;
        public ISamplerState Sampler;
        public IO.Materials.MaterialTexture Desc;
        private bool disposedValue;

        public MaterialTexture(IShaderResourceView srv, ISamplerState sampler, IO.Materials.MaterialTexture desc)
        {
            ShaderResourceView = srv;
            Sampler = sampler;
            Desc = desc;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
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