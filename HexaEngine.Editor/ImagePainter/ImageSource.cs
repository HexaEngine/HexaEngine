namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class ImageSource : IDisposable
    {
        private readonly ITexture2D[] textures;
        private readonly IShaderResourceView[] srvs;
        private readonly IRenderTargetView[] rtvs;
        private readonly Viewport[] viewports;
        private int arrayIndex;
        private int mipLevel;
        private int index;
        private TexMetadata metadata;
        private TexMetadata originalMetadata;
        private bool disposedValue;

        public ImageSource(IGraphicsDevice device, IScratchImage image, TexMetadata originalMetadata)
        {
            this.originalMetadata = originalMetadata;
            metadata = image.Metadata;
            var count = image.ImageCount;
            textures = new ITexture2D[count];
            srvs = new IShaderResourceView[count];
            rtvs = new IRenderTargetView[count];
            viewports = new Viewport[count];

            for (var i = 0; i < count; i++)
            {
                textures[i] = image.CreateTexture2D(device, i, Usage.Default, BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags.None, ResourceMiscFlag.None);
                var desc = textures[i].Description;
                srvs[i] = device.CreateShaderResourceView(textures[i]);
                rtvs[i] = device.CreateRenderTargetView(textures[i]);
                viewports[i] = new(desc.Width, desc.Height);
            }
        }

        public TexMetadata Metadata => metadata;

        public TexMetadata OriginalMetadata { get => originalMetadata; set => originalMetadata = value; }

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

        public Viewport Viewport => viewports[index];

        public ITexture2D[] Textures => textures;

        public IShaderResourceView[] SRVs => srvs;

        public IRenderTargetView[] RTVs => rtvs;

        public Viewport[] Viewports => viewports;

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
                Memcpy(imageSrc.Pixels, imageDst.Pixels, imageSrc.RowPitch * imageSrc.Height);
                capture.Dispose();
            }

            return scratchImage;
        }

        public unsafe Vector4 GetPixel(IGraphicsDevice device, Point2 position)
        {
            IScratchImage scratchImage = device.TextureLoader.CaptureTexture(device.Context, textures[index]);
            IScratchImage newScratchImage = scratchImage.Convert(Format.R32G32B32A32Float, TexFilterFlags.Default);
            scratchImage.Dispose();

            TexMetadata metadata = newScratchImage.Metadata;

            IImage image = newScratchImage.GetImages()[0];
            Vector4* pixels = (Vector4*)image.Pixels;

            int idx = position.Y * metadata.Width + position.X;

            Vector4 pixel = pixels[idx];

            newScratchImage.Dispose();
            return pixel;
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}