namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using System;

    public unsafe class DXGISwapChain : DisposableBase, ISwapChain
    {
        private IDXGISwapChain1* swapChain;
        private SwapChainFlag flags;
        private ID3D11Texture2D* backbuffer;
        private ITexture2D depthStencil;

        internal DXGISwapChain(D3D11GraphicsDevice device, SwapChainFlag flags)
        {
            Device = device;
            swapChain = device.swapChain;
            this.flags = flags;

            ID3D11Texture2D* backbuffer;
            swapChain->GetBuffer(0, Utils.Guid(ID3D11Texture2D.Guid), (void**)&backbuffer);
            Texture2DDesc desc;
            backbuffer->GetDesc(&desc);
            Texture2DDescription description = Helper.ConvertBack(desc);
            this.backbuffer = backbuffer;

            Backbuffer = new D3D11Texture2D(backbuffer, description);
            BackbufferRTV = device.CreateRenderTargetView(Backbuffer, new(description.Width, description.Height));
            depthStencil = device.CreateTexture2D(Core.Graphics.Format.Depth24UNormStencil8, description.Width, description.Height, 1, 1, null, BindFlags.DepthStencil);
            BackbufferDSV = device.CreateDepthStencilView(depthStencil);
            Width = description.Width;
            Height = description.Height;
            Viewport = new(0, 0, Width, Height);
        }

        public ITexture2D Backbuffer { get; private set; }

        public IRenderTargetView BackbufferRTV { get; private set; }

        public IDepthStencilView BackbufferDSV { get; private set; }

        public IGraphicsDevice Device { get; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Mathematics.Viewport Viewport { get; private set; }

        public event EventHandler? Resizing;

        public event EventHandler<ResizedEventArgs>? Resized;

        public void Present(uint syncInt)
        {
            swapChain->Present(syncInt, 0);
        }

        public void Resize(int width, int height)
        {
            var oldWidth = Width;
            var oldHeight = Height;
            Resizing?.Invoke(this, EventArgs.Empty);

            Backbuffer.Dispose();
            BackbufferRTV.Dispose();
            BackbufferDSV.Dispose();
            depthStencil.Dispose();

            swapChain->ResizeBuffers(2, (uint)width, (uint)height, Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm, (uint)flags);
            Width = width;
            Height = height;
            Viewport = new(0, 0, Width, Height);

            ID3D11Texture2D* backbuffer;
            swapChain->GetBuffer(0, Utils.Guid(ID3D11Texture2D.Guid), (void**)&backbuffer);
            Texture2DDesc desc;
            backbuffer->GetDesc(&desc);
            Texture2DDescription description = Helper.ConvertBack(desc);
            this.backbuffer = backbuffer;

            Backbuffer = new D3D11Texture2D(backbuffer, description);
            BackbufferRTV = Device.CreateRenderTargetView(Backbuffer, new(description.Width, description.Height));
            depthStencil = Device.CreateTexture2D(Core.Graphics.Format.Depth24UNormStencil8, description.Width, description.Height, 1, 1, null, BindFlags.DepthStencil);
            BackbufferDSV = Device.CreateDepthStencilView(depthStencil);

            Resized?.Invoke(this, new(oldWidth, oldHeight, width, height));
        }

        protected override void DisposeCore()
        {
            Backbuffer.Dispose();
            BackbufferRTV.Dispose();
            BackbufferDSV.Dispose();
            depthStencil.Dispose();
            swapChain->Release();
        }
    }
}