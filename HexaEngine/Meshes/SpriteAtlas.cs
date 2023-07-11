namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Graphics;
    using System.Numerics;

    public class SpriteAtlas : IDisposable
    {
        private readonly Texture2D texture;
        private readonly ISamplerState samplerState;

        public SpriteAtlas(IGraphicsDevice device, SamplerDescription description, string path)
        {
            samplerState = device.CreateSamplerState(description);
            texture = new(device, new TextureFileDescription(path, TextureDimension.Texture2D, 0, Usage.Immutable));
        }

        public Vector2 Size => new(texture.Width, texture.Height);

        public IShaderResourceView SRV => texture.SRV;

        public ISamplerState SamplerState => samplerState;

        public void Dispose()
        {
            texture.Dispose();
            samplerState.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}