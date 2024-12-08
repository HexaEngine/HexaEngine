namespace HexaEngine.Meshes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;

    public class Skybox : IDisposable
    {
        private readonly SamplerState samplerState;
        private Texture2D? environment;

        private bool loaded;
        private bool disposedValue;

        public Skybox()
        {
            samplerState = new(SamplerStateDescription.LinearWrap);
        }

        public ISamplerState SamplerState => samplerState;

        public Texture2D? Environment => environment;

        public bool Loaded => loaded;

        public void Load(string environmentPath)
        {
            environment = new(new TextureFileDescription(environmentPath, TextureDimension.TextureCube, 0, Usage.Immutable));
            loaded = true;
        }

        public Task LoadAsync(AssetRef asset)
        {
            environment = Texture2D.LoadFromAssets(asset, TextureDimension.TextureCube);
            loaded = true;
            return Task.CompletedTask;
        }

        public void Unload()
        {
            environment?.Dispose();
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}