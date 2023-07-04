namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.Graphics;

    public class Skybox : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly ISamplerState samplerState;
        private Texture2D? environment;

        private bool loaded;
        private bool disposedValue;

        public Skybox(IGraphicsDevice device)
        {
            this.device = device;
            samplerState = device.CreateSamplerState(SamplerDescription.LinearWrap);
        }

        public ISamplerState SamplerState => samplerState;

        public Texture2D? Environment => environment;

        public bool Loaded => loaded;

        public void Load(string environmentPath)
        {
            environment = new(device, new TextureFileDescription(environmentPath, TextureDimension.TextureCube, 0, Usage.Immutable));
            loaded = true;
        }

        public async Task LoadAsync(string environmentPath)
        {
            environment = await Texture2D.CreateTextureAsync(device, new(environmentPath, TextureDimension.TextureCube));
            loaded = true;
        }

        public void Unload()
        {
            environment.Dispose();
            loaded = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Unload();
                disposedValue = true;
            }
        }

        ~Skybox()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
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