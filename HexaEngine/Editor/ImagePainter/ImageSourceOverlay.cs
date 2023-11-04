namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering.Renderers;

    public class ImageSourceOverlay
    {
        private readonly Texture2D[] textures;
        private readonly IShaderResourceView[] srvs;
        private readonly IRenderTargetView[] rtvs;
        private readonly DepthStencil[] depths;
        private readonly IDepthStencilView[] dsvs;
        private readonly Viewport[] viewports;
        private readonly ImageSource image;
        private bool disposedValue;

        public ImageSourceOverlay(IGraphicsDevice device, ImageSource image, ISamplerState sampler)
        {
            var count = image.ImageCount;
            textures = new Texture2D[count];
            srvs = new IShaderResourceView[count];
            rtvs = new IRenderTargetView[count];
            depths = new DepthStencil[count];
            dsvs = new IDepthStencilView[count];
            viewports = new Viewport[count];

            for (var i = 0; i < count; i++)
            {
                var desc = image.Textures[i].Description;
                textures[i] = new(device, desc);
                srvs[i] = textures[i].SRV;
                rtvs[i] = textures[i].RTV;
                viewports[i] = new(desc.Width, desc.Height);
                ImGuiRenderer.Samplers.Add(srvs[i].NativePointer, sampler);
                desc.BindFlags = BindFlags.DepthStencil;
                desc.Usage = Usage.Default;
                desc.CPUAccessFlags = CpuAccessFlags.None;
                desc.Format = Format.D16UNorm;
                depths[i] = new(device, Format.D32Float, desc.Width, desc.Height, desc.ArraySize);
                dsvs[i] = depths[i].DSV;
            }

            this.image = image;
        }

        public TexMetadata Metadata => image.Metadata;

        public int Index => image.Index;

        public int ArrayIndex { get => image.ArrayIndex; set => image.ArrayIndex = value; }

        public int ArraySize => image.ArraySize;

        public int MipLevel { get => image.MipLevel; set => image.MipLevel = value; }

        public int MipLevels => image.MipLevels;

        public ITexture2D Texture => textures[image.Index];

        public IShaderResourceView SRV => srvs[image.Index];

        public IRenderTargetView RTV => rtvs[image.Index];

        public IDepthStencilView DSV => dsvs[image.Index];

        public Viewport Viewport => viewports[image.Index];

        public ITexture2D[] Textures => textures;

        public IShaderResourceView[] SRVs => srvs;

        public IRenderTargetView[] RTVs => rtvs;

        public IDepthStencilView[] DSVs => dsvs;

        public Viewport[] Viewports => viewports;

        public int ImageCount => textures.Length;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    for (int i = 0; i < textures.Length; i++)
                    {
                        ImGuiRenderer.Samplers.Remove(srvs[i].NativePointer);
                        textures[i].Dispose();
                        srvs[i].Dispose();
                        rtvs[i].Dispose();
                        depths[i].Dispose();
                        dsvs[i].Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        ~ImageSourceOverlay()
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