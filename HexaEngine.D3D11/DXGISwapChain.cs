namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Logging;
    using HexaEngine.Core.Windows.Events;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public unsafe partial class DXGISwapChain : DeviceChildBase, ISwapChain
    {
        private D3D11GraphicsDevice device;
        private ComPtr<IDXGISwapChain2> swapChain;
        private readonly SwapChainFlag flags;
        private ComPtr<ID3D11Texture2D> backbuffer;
        private ITexture2D depthStencil;
        private long fpsStartTime;
        private long fpsFrameCount;
        private bool vSync;
        private bool limitFPS;
        private int targetFPS = 120;
        private bool active;

        internal DXGISwapChain(D3D11GraphicsDevice device, ComPtr<IDXGISwapChain2> swapChain, SwapChainFlag flags)
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
            ResultCode hr;
            if (sync)
            {
                hr = (ResultCode)swapChain.Present(1, 0);
            }
            else
            {
                hr = (ResultCode)swapChain.Present(0, DXGI.PresentAllowTearing);
            }
            CheckError(hr);
        }

        public void Present()
        {
            ResultCode hr;
            if (!active)
            {
                hr = (ResultCode)swapChain.Present(4, 0);
            }
            else if (vSync)
            {
                hr = (ResultCode)swapChain.Present(1, 0);
            }
            else
            {
                hr = (ResultCode)swapChain.Present(0, DXGI.PresentAllowTearing);
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

            ResultCode code = (ResultCode)swapChain.ResizeBuffers(0, (uint)width, (uint)height, Silk.NET.DXGI.Format.FormatUnknown, (uint)flags);

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
            depthStencil = Device.CreateTexture2D(Core.Graphics.Format.D32FloatS8X24UInt, description.Width, description.Height, 1, 1, null, BindFlags.DepthStencil);
            BackbufferDSV = Device.CreateDepthStencilView(depthStencil);

            Resized?.Invoke(this, new(oldWidth, oldHeight, width, height));
        }

        private bool CheckError(ResultCode hr)
        {
            if (hr == ResultCode.DXGI_ERROR_DEVICE_REMOVED || hr == ResultCode.DXGI_ERROR_DEVICE_RESET)
            {
                var reason = device.Device.GetDeviceRemovedReason();
                DeviceRemovedEventArgs e = new($"Device removed! DXGI_ERROR code: {(ResultCode)reason}", reason);
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