namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;

    public class ImageSource : IDisposable
    {
        private readonly ITexture2D[] textures;
        private readonly IShaderResourceView[] srvs;
        private readonly IRenderTargetView[] rtvs;
        private int arrayIndex;
        private int mipLevel;
        private int index;
        private TexMetadata metadata;
        private bool disposedValue;

        public ImageSource(IGraphicsDevice device, IScratchImage image)
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

        public TexMetadata Metadata => metadata;

        public int Index => index;

        public int ArrayIndex
        {
            get => arrayIndex;
            set
            {
                arrayIndex = value;
                index = metadata.ComputeIndex(mipLevel, arrayIndex, 0);
            }
        }

        public int ArraySize => metadata.ArraySize;

        public int MipLevel
        {
            get => mipLevel;
            set
            {
                mipLevel = value;
                index = metadata.ComputeIndex(mipLevel, arrayIndex, 0);
            }
        }

        public int MipLevels => metadata.MipLevels;

        public ITexture2D Texture => textures[index];

        public IShaderResourceView SRV => srvs[index];

        public IRenderTargetView RTV => rtvs[index];

        public ITexture2D[] Textures => textures;

        public IShaderResourceView[] SRVs => srvs;

        public IRenderTargetView[] RTVs => rtvs;

        public int ImageCount => textures.Length;

        public unsafe IScratchImage ToScratchImage(IGraphicsDevice device)
        {
            IScratchImage scratchImage;

            if (metadata.IsCubemap())
            {
                scratchImage = device.TextureLoader.InitializeCube(metadata.Format, metadata.Width, metadata.Height, metadata.ArraySize / 6, metadata.MipLevels);
            }
            else
            {
                scratchImage = device.TextureLoader.Initialize2D(metadata.Format, metadata.Width, metadata.Height, metadata.ArraySize, metadata.MipLevels);
            }

            var images = scratchImage.GetImages();
            for (int i = 0; i < textures.Length; i++)
            {
                var capture = device.TextureLoader.CaptureTexture(device.Context, textures[i]);
                var imageSrc = capture.GetImages()[0];
                var imageDst = images[i];
                Memcpy((void*)imageSrc.Pixels, imageDst.Pixels, imageSrc.RowPitch * imageSrc.Height);
                capture.Dispose();
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

        ~ImageSource()
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