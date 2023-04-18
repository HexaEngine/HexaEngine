namespace HexaEngine.D3D12
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows.Events;
    using Silk.NET.Direct3D12;
    using Silk.NET.DXGI;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public unsafe class DXGISwapChain : DeviceChildBase, ISwapChain
    {
        private IDXGISwapChain3* swapChain;
        private readonly uint bufferCount;
        private SwapChainFlag flags;
        private uint frameIndex;
        private ID3D12DescriptorHeap* descriptorHeap;
        private ID3D12Resource** backbuffers;

        private ITexture2D depthStencil;
        private long fpsStartTime;
        private long fpsFrameCount;
        private bool vSync;
        private bool limitFPS;
        private int targetFPS = 120;
        private bool active;

        internal DXGISwapChain(D3D12GraphicsDevice device, IDXGISwapChain3* swapChain, int width, int height, uint bufferCount, SwapChainFlag flags)
        {
            Device = device;
            this.swapChain = swapChain;
            this.bufferCount = bufferCount;
            this.flags = flags;

            frameIndex = swapChain->GetCurrentBackBufferIndex();

            DescriptorHeapDesc descriptorHeapDesc = new()
            {
                Type = DescriptorHeapType.Rtv,
                Flags = DescriptorHeapFlags.None,
                NumDescriptors = bufferCount,
            };

            ID3D12DescriptorHeap* heap;
            device.Device.CreateDescriptorHeap(&descriptorHeapDesc, Utils.Guid(ID3D12DescriptorHeap.Guid), (void**)&heap).ThrowHResult();
            descriptorHeap = heap;

            uint rtvDescriptorSize = device.Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.Rtv);

            CpuDescriptorHandle rtvHandle = heap->GetCPUDescriptorHandleForHeapStart();

            backbuffers = (ID3D12Resource**)AllocArray(bufferCount);
            for (uint i = 0; i < bufferCount; i++)
            {
                swapChain->GetBuffer(i, Utils.Guid(ID3D12Resource.Guid), (void**)&backbuffers[i]);
                device.Device.CreateRenderTargetView(backbuffers[i], (RenderTargetViewDesc*)null, rtvHandle);
                rtvHandle.Ptr += rtvDescriptorSize;
            }

            depthStencil = device.CreateTexture2D(Core.Graphics.Format.Depth24UNormStencil8, width, height, 1, 1, null, BindFlags.DepthStencil);
            BackbufferDSV = device.CreateDepthStencilView(depthStencil);
            Width = width;
            Height = height;
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

        public bool VSync { get => vSync; set => vSync = value; }

        public bool LimitFPS { get => limitFPS; set => limitFPS = value; }

        public int TargetFPS { get => targetFPS; set => targetFPS = value; }

        public bool Active { get => active; set => active = value; }

        public void Present(bool sync)
        {
            if (sync)
            {
                swapChain->Present(1, 0);
            }
            else
            {
                swapChain->Present(0, DXGI.PresentAllowTearing);
            }
        }

        public void Present()
        {
            if (!active)
            {
                swapChain->Present(4, 0);
            }
            else if (vSync)
            {
                swapChain->Present(1, 0);
            }
            else
            {
                swapChain->Present(0, DXGI.PresentAllowTearing);
            }
        }

        public void Wait()
        {
            if (!vSync && limitFPS)
            {
                LimitFrameRate();
            }
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
                if (sleepTime > 0) Thread.Sleep(sleepTime);
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

            swapChain->ResizeBuffers(2, (uint)width, (uint)height, Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm, (uint)flags);
            Width = width;
            Height = height;
            Viewport = new(0, 0, Width, Height);

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