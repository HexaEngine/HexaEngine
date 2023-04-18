namespace ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;

    public class Image : IDisposable
    {
        private readonly ITexture2D[] textures;
        private readonly IShaderResourceView[] srvs;
        private readonly IRenderTargetView[] rtvs;
        private int arrayIndex;
        private int mipMap;
        private int index;
        private TexMetadata metadata;
        private bool disposedValue;

        public Image(IGraphicsDevice device, IScratchImage image)
        {
            metadata = image.Metadata;
            var count = image.ImageCount;
            textures = new ITexture2D[count];
            srvs = new IShaderResourceView[count];
            rtvs = new IRenderTargetView[count];

            for (var i = 0; i < count; i++)
            {
                textures[i] = image.CreateTexture2D(device, i, Usage.Default, BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags.None, ResourceMiscFlag.None);
                var desc = textures[i].Description;
                srvs[i] = device.CreateShaderResourceView(textures[i]);
                rtvs[i] = device.CreateRenderTargetView(textures[i], new(desc.Width, desc.Height));
            }
        }

        public int ArrayIndex
        {
            get => arrayIndex;
            set
            {
                arrayIndex = value;
                index = metadata.ComputeIndex(mipMap, arrayIndex, 0);
            }
        }

        public int ArraySize => metadata.ArraySize;

        public int MipMap
        {
            get => mipMap;
            set
            {
                mipMap = value;
                index = metadata.ComputeIndex(mipMap, arrayIndex, 0);
            }
        }

        public int MipLevels => metadata.MipLevels;

        public IShaderResourceView SRV => srvs[index];

        public IRenderTargetView RTV => rtvs[index];

        public IScratchImage Convert(IGraphicsDevice device)
        {
            IScratchImage[] scratchImages = new IScratchImage[textures.Length];
            IImage[] images = new IImage[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                scratchImages[i] = device.TextureLoader.CaptureTexture(device.Context, textures[i]);
                images[i] = scratchImages[i].GetImages()[0];
            }

            var scratchImage = device.TextureLoader.InitializeArrayFromImages(images);

            for (int i = 0; i < textures.Length; i++)
            {
                scratchImages[i].Dispose();
            }

            return scratchImage;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    for (int i = 0; i < textures.Length; i++)
                    {
                        textures[i].Dispose();
                        srvs[i].Dispose();
                        rtvs[i].Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        ~Image()
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