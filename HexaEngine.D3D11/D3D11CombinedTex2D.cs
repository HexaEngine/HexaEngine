namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11CombinedTex2D : DeviceChildBase, ICombinedTex2D
    {
        internal ComPtr<ID3D11Texture2D> tex;
        internal ComPtr<ID3D11ShaderResourceView> srv;
        internal ComPtr<ID3D11RenderTargetView> rtv;
        internal ComPtr<ID3D11UnorderedAccessView> uav;
        internal ComPtr<ID3D11DepthStencilView> dsv;
        private HexaEngine.Mathematics.Viewport viewport;

        public D3D11CombinedTex2D(D3D11GraphicsDevice device, CombinedTex2DDesc description)
        {
            var dev = device.Device;

            var texDesc = Helper.Convert(description);

            dev.CreateTexture2D(texDesc, null, ref tex).ThrowHResult();
            nativePointer = new(tex.Handle);

            var flags = (BindFlag)texDesc.BindFlags;

            if (flags.HasFlag(BindFlag.RenderTarget))
            {
                dev.CreateRenderTargetView(tex, null, ref rtv).ThrowHResult();
            }

            if (flags.HasFlag(BindFlag.ShaderResource))
            {
                dev.CreateShaderResourceView(tex, null, ref srv).ThrowHResult();
            }

            if (flags.HasFlag(BindFlag.UnorderedAccess))
            {
                dev.CreateUnorderedAccessView(tex, null, ref uav).ThrowHResult();
            }

            if (flags.HasFlag(BindFlag.DepthStencil))
            {
                dev.CreateDepthStencilView(tex, null, ref dsv).ThrowHResult();
            }

            viewport = new(texDesc.Width, texDesc.Height);
        }

        public bool IsSRV => srv.Handle != null;

        public bool IsRTV => rtv.Handle != null;

        public bool IsUAV => uav.Handle != null;

        public bool IsDSV => dsv.Handle != null;

        public Mathematics.Viewport Viewport => viewport;

        protected override void DisposeCore()
        {
            if (tex.Handle != null)
            {
                tex.Release();
                tex = null;
            }
            if (rtv.Handle != null)
            {
                rtv.Release();
                rtv = null;
            }
            if (srv.Handle != null)
            {
                srv.Release();
                srv = null;
            }
            if (uav.Handle != null)
            {
                uav.Release();
                uav = null;
            }
            if (dsv.Handle != null)
            {
                dsv.Release();
                dsv = null;
            }
        }
    }
}