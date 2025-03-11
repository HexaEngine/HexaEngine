namespace HexaEngine.D3D11
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows.Events;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public unsafe partial class DXGISwapChain : DeviceChildBase, ISwapChain
    {
        private D3D11GraphicsDevice device;
        private ComPtr<IDXGISwapChain3> swapChain;
        private readonly SwapChainFlag flags;
        private ComPtr<ID3D11Texture2D> backbuffer;
        private ITexture2D depthStencil;
        private long fpsStartTime;
        private long fpsFrameCount;
        private bool vSync;
        private bool limitFPS;
        private int targetFPS = 120;
        private bool active;

        internal DXGISwapChain(D3D11GraphicsDevice device, ComPtr<IDXGISwapChain3> swapChain, SwapChainFlag flags)
        {
            this.device = device;
            this.swapChain = swapChain;
            this.flags = flags;

            swapChain.GetBuffer(0, out backbuffer);
            Texture2DDesc desc;
            backbuffer.GetDesc(&desc);
            Texture2DDescription description = Helper.ConvertBack(desc);

            Backbuffer = new D3D11Texture2D(backbuffer, description);
            BackbufferRTV = device.CreateRenderTargetView(Backbuffer);
            depthStencil = device.CreateTexture2D(Core.Graphics.Format.D32FloatS8X24UInt, description.Width, description.Height, 1, 1, null, BindFlags.DepthStencil);
            BackbufferDSV = device.CreateDepthStencilView(depthStencil);
            Width = description.Width;
            Height = description.Height;
            Viewport = new(0, 0, Width, Height);
        }

        public ITexture2D Backbuffer { get; private set; }

        public IRenderTargetView BackbufferRTV { get; private set; }

        public IDepthStencilView BackbufferDSV { get; private set; }

        public IGraphicsDevice Device => device;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Hexa.NET.Mathematics.Viewport Viewport { get; private set; }

        public event EventHandler? Resizing;

        public event EventHandler<ResizedEventArgs>? Resized;

        public event EventHandler<DeviceRemovedEventArgs>? DeviceRemoved;

        public bool VSync { get => vSync; set => vSync = value; }

        public bool LimitFPS { get => limitFPS; set => limitFPS = value; }

        public int TargetFPS { get => targetFPS; set => targetFPS = value; }

        public bool Active { get => active; set => active = value; }

        public void Present(bool sync)
        {
            HResult hr;
            if (sync)
            {
                hr = swapChain.Present(1, 0);
            }
            else
            {
                hr = swapChain.Present(0, (uint)DXGI.DXGI_PRESENT_ALLOW_TEARING);
            }
            CheckError(hr);
        }

        public void Present()
        {
            HResult hr;
            if (!active)
            {
                hr = swapChain.Present(1, 0);
            }
            else if (vSync)
            {
                hr = swapChain.Present(1, 0);
            }
            else
            {
                hr = swapChain.Present(0, (uint)DXGI.DXGI_PRESENT_ALLOW_TEARING);
            }
            CheckError(hr);
        }

        public void Wait()
        {
            if (!vSync && limitFPS)
            {
                LimitFrameRate();
            }
        }

        public void WaitForPresent()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LimitFrameRate()
        {
            int fps = targetFPS;
            long freq = Stopwatch.Frequency;
            long frame = Stopwatch.GetTimestamp();
            while ((frame - fpsStartTime) * fps < freq * fpsFrameCount)
            {
                int sleepTime = (int)((fpsStartTime * fps + freq * fpsFrameCount - frame * fps) * 1000 / (freq * fps));
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }

                frame = Stopwatch.GetTimestamp();
            }
            if (++fpsFrameCount > fps)
            {
                fpsFrameCount = 0;
                fpsStartTime = frame;
            }
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

            HResult code = swapChain.ResizeBuffers(0, (uint)width, (uint)height, Hexa.NET.DXGI.Format.Unknown, (uint)flags);

            if (CheckError(code))
            {
                return;
            }

            Width = width;
            Height = height;
            Viewport = new(0, 0, Width, Height);

            swapChain.GetBuffer(0, out backbuffer);
            Texture2DDesc desc;
            backbuffer.GetDesc(&desc);
            Texture2DDescription description = Helper.ConvertBack(desc);

            Backbuffer = new D3D11Texture2D(backbuffer, description);
            BackbufferRTV = Device.CreateRenderTargetView(Backbuffer);
            depthStencil = Device.CreateTexture2D(Format.D32FloatS8X24UInt, description.Width, description.Height, 1, 1, null, BindFlags.DepthStencil);
            BackbufferDSV = Device.CreateDepthStencilView(depthStencil);

            Resized?.Invoke(this, new(oldWidth, oldHeight, width, height));
        }

        public void SetColorSpace(ColorSpace colorSpace)
        {
            swapChain.SetColorSpace1(Helper.Convert(colorSpace));
        }

        private bool CheckError(HResult hr)
        {
            ResultCode result = (ResultCode)hr.Value;
            if (result == ResultCode.DXGI_ERROR_DEVICE_REMOVED || result == ResultCode.DXGI_ERROR_DEVICE_RESET)
            {
                var reason = device.Device.GetDeviceRemovedReason();
                DeviceRemovedEventArgs e = new($"Device removed! DXGI_ERROR code: {(ResultCode)reason.Value}", reason);
                LoggerFactory.GetLogger(nameof(DXGI)).Error(e);
                DeviceRemoved?.Invoke(this, e);
                return true;
            }
            return false;
        }

        protected override void DisposeCore()
        {
            Backbuffer.Dispose();
            BackbufferRTV.Dispose();
            BackbufferDSV.Dispose();
            depthStencil.Dispose();
            swapChain.Release();
        }
    }
}