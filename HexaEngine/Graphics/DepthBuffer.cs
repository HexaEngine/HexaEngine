namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public class DepthBuffer
    {
        public readonly IResource Resource;
        public readonly IDepthStencilView DSV;
        public readonly IShaderResourceView? SRV;
        public readonly Viewport Viewport;

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
    }
}