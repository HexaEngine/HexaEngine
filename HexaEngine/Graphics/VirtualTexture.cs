namespace HexaEngine.Graphics
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;

    public enum VirtualTextureFlags
    {
        None,
        UseCache,
    }

    public class VirtualTexture : DisposableBase
    {
        private readonly Format format;
        private readonly int width;
        private readonly int height;
        private readonly int mipLevels;
        private readonly int tileSize;
        private readonly VirtualTextureFlags flags;
        private Texture2D texture;

        private int widthTiles;
        private int heightTiles;
        private int tileCount;

        public VirtualTexture(IGraphicsDevice device, Format format, int width, int height, int mipLevels, int tileSize, VirtualTextureFlags flags = VirtualTextureFlags.UseCache)
        {
            if (width % tileSize != 0 || height % tileSize != 0)
            {
                throw new ArgumentException($"Width and height must be dividable by tile size.");
            }

            if (FormatHelper.IsCompressed(format))
            {
                throw new ArgumentException("Cannot use compressed texture format");
            }

            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.tileSize = tileSize;
            this.flags = flags;
            texture = new(device, format, width, height, 1, mipLevels, CpuAccessFlags.None, GpuAccessFlags.Read | GpuAccessFlags.UA);

            widthTiles = width / tileSize;
            heightTiles = height / tileSize;
            tileCount = widthTiles * heightTiles;
        }

        protected override void DisposeCore()
        {
            texture.Dispose();
        }
    }
}