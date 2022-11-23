namespace HexaEngine.Graphics
{
    using BepuPhysics.Trees;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public class DepthBuffer : IDisposable
    {
        public readonly IResource Resource;
        public readonly IDepthStencilView DSV;
        public readonly IShaderResourceView? SRV;
        public readonly Viewport Viewport;
        private bool disposedValue;

        public DepthBuffer(IGraphicsDevice device, DepthStencilBufferDesc desc)
        {
            Format format = desc.Format;
            if (desc.BindFlags.HasFlag(BindFlags.ShaderResource))
                format = Format.R32Typeless;

            Texture2DDescription depthStencilDesc = new(
            format,
            desc.Width,
            desc.Height,
            desc.ArraySize,
            1,
            desc.BindFlags,
            desc.Usage,
            desc.CPUAccessFlags,
            desc.SampleDescription.Count,
            desc.SampleDescription.Quality,
            ResourceMiscFlag.None);

            Viewport = new(desc.Width, desc.Height);

            Resource = device.CreateTexture2D(depthStencilDesc);
            var dsvdesc = new DepthStencilViewDescription((ITexture2D)Resource, desc.ArraySize > 1 ? DepthStencilViewDimension.Texture2DArray : DepthStencilViewDimension.Texture2D);
            dsvdesc.Format = desc.Format;
            DSV = device.CreateDepthStencilView(Resource, dsvdesc);
            if (desc.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                var srvdesc = new ShaderResourceViewDescription((ITexture2D)Resource, desc.ArraySize > 1 ? ShaderResourceViewDimension.Texture2DArray : ShaderResourceViewDimension.Texture2D);
                srvdesc.Format = Format.R32Float;
                SRV = device.CreateShaderResourceView(Resource, srvdesc);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Resource.Dispose();
                DSV.Dispose();
                SRV?.Dispose();

                disposedValue = true;
            }
        }

        ~DepthBuffer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}