﻿namespace HexaEngine.Core.Graphics
{
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
            var dsvdesc = new DepthStencilViewDescription((ITexture2D)Resource, desc.ArraySize > 1 ? DepthStencilViewDimension.Texture2DArray : DepthStencilViewDimension.Texture2D, Format.Depth32Float);
            dsvdesc.Format = desc.Format;
            DSV = device.CreateDepthStencilView(Resource, dsvdesc);
            if (desc.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                var srvdesc = new ShaderResourceViewDescription((ITexture2D)Resource, desc.ArraySize > 1 ? ShaderResourceViewDimension.Texture2DArray : ShaderResourceViewDimension.Texture2D);
                srvdesc.Format = Format.R32Float;
                SRV = device.CreateShaderResourceView(Resource, srvdesc);
            }
        }

        public DepthBuffer(IGraphicsDevice device, DepthStencilBufferDesc desc, int mips)
        {
            Format format = desc.Format;
            if (desc.BindFlags.HasFlag(BindFlags.ShaderResource))
                format = Format.R32Typeless;

            Texture2DDescription depthStencilDesc = new(
            format,
            desc.Width,
            desc.Height,
            1,
            mips,
            desc.BindFlags,
            desc.Usage,
            desc.CPUAccessFlags,
            desc.SampleDescription.Count,
            desc.SampleDescription.Quality,
            ResourceMiscFlag.None);

            Viewport = new(desc.Width, desc.Height);

            Resource = device.CreateTexture2D(depthStencilDesc);
            var dsvdesc = new DepthStencilViewDescription((ITexture2D)Resource, DepthStencilViewDimension.Texture2D);
            dsvdesc.Format = desc.Format;
            DSV = device.CreateDepthStencilView(Resource, dsvdesc);
            if (desc.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                var srvdesc = new ShaderResourceViewDescription((ITexture2D)Resource, ShaderResourceViewDimension.Texture2D);
                srvdesc.Format = Format.R32Float;
                SRV = device.CreateShaderResourceView(Resource, srvdesc);
            }
        }

        public DepthBuffer(IGraphicsDevice device, int width, int height, Format format, BindFlags flags = BindFlags.ShaderResource | BindFlags.DepthStencil)
        {
            Format resourceFormat = GetDepthResourceFormat(format);
            Format srvFormat = GetDepthSRVFormat(format);
            Texture2DDescription depthStencilDesc = new(resourceFormat, width, height, 1, 1, flags);

            Viewport = new(width, height);

            Resource = device.CreateTexture2D(depthStencilDesc);
            DSV = device.CreateDepthStencilView(Resource, new((ITexture2D)Resource, DepthStencilViewDimension.Texture2D, format));
            if (flags.HasFlag(BindFlags.ShaderResource))
            {
                SRV = device.CreateShaderResourceView(Resource, new((ITexture2D)Resource, ShaderResourceViewDimension.Texture2D, srvFormat));
            }
        }

        private static Format GetDepthResourceFormat(Format depthformat)
        {
            Format resformat = Format.Unknown;
            switch (depthformat)
            {
                case Format.Depth16UNorm:
                    resformat = Format.R32Typeless;
                    break;

                case Format.Depth24UNormStencil8:
                    resformat = Format.R24G8Typeless;
                    break;

                case Format.Depth32Float:
                    resformat = Format.R32Typeless;
                    break;

                case Format.Depth32FloatStencil8:
                    resformat = Format.R32G8X24Typeless;
                    break;
            }

            return resformat;
        }

        private static Format GetDepthSRVFormat(Format depthformat)
        {
            Format srvformat = Format.Unknown;
            switch (depthformat)
            {
                case Format.Depth16UNorm:
                    srvformat = Format.R16Float;
                    break;

                case Format.Depth24UNormStencil8:
                    srvformat = Format.R24UNormX8Typeless;
                    break;

                case Format.Depth32Float:
                    srvformat = Format.R32Float;
                    break;

                case Format.Depth32FloatStencil8X24UInt:
                    srvformat = Format.R32FloatX8X24Typeless;
                    break;
            }
            return srvformat;
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