﻿namespace HexaEngine.D3D12
{
    using HexaEngine.Core.Debugging.Device;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Logging;
    using HexaEngine.Core.Windows;
    using Silk.NET.Core.Native;
    using Silk.NET.DXGI;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Versioning;

    public unsafe class DXGIAdapterD3D12 : IGraphicsAdapter
    {
        internal readonly DXGI DXGI;

        internal IDXGIFactory4* IDXGIFactory;
        internal ComPtr<IDXGIAdapter1> IDXGIAdapter;
        internal ComPtr<IDXGIAdapter3> IDXGIAdapter3;

        public DXGIAdapterD3D12()
        {
            DXGI = DXGI.GetApi();

            IDXGIFactory4* factory;
            DXGI.CreateDXGIFactory1(Utils.Guid(IDXGIFactory4.Guid), (void**)&factory);
            IDXGIFactory = factory;

            IDXGIAdapter = GetHardwareAdapter();
            IDXGIAdapter.QueryInterface(out IDXGIAdapter3);
        }

        public static void Init(IWindow window, bool graphicsDebugging)
        {
            if (OperatingSystem.IsWindows())
            {
                GraphicsAdapter.Adapters.Add(new DXGIAdapterD3D12());
            }
        }

        public ulong GetMemoryBudget()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter3.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.Budget;
        }

        public ulong GetMemoryCurrentUsage()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter3.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.CurrentUsage;
        }

        public ulong GetMemoryAvailableForReservation()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter3.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.AvailableForReservation;
        }

        public ulong GetMemoryCurrentReservation()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter3.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.AvailableForReservation;
        }

        public virtual GraphicsBackend Backend => GraphicsBackend.D3D11;

        public virtual int PlatformScore => 100;

        public IReadOnlyList<GPU> GPUs { get; }

        public int AdapterIndex { get; private set; }

        [SupportedOSPlatform("windows")]
        public virtual IGraphicsDevice CreateGraphicsDevice(bool debug)
        {
            AdapterDesc1 desc;
            IDXGIAdapter.GetDesc1(&desc);
            string name = new(desc.Description);

            LoggerFactory.General.Info("Backend: Using Graphics API: D3D11");
            LoggerFactory.General.Info($"Backend: Using Graphics Device: {name}");
            return new D3D12GraphicsDevice(this, debug);
        }

        [SupportedOSPlatform("windows")]
        internal ISwapChain CreateSwapChainForWindow(D3D12GraphicsDevice device, SdlWindow window)
        {
            var (Hwnd, HDC, HInstance) = window.Win32 ?? throw new NotSupportedException();

            SwapChainDesc desc = new()
            {
                BufferCount = 2,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SampleDesc = new(1, 0),
                SwapEffect = Silk.NET.DXGI.SwapEffect.FlipSequential,
                Flags = (uint)(SwapChainFlag.AllowModeSwitch | SwapChainFlag.AllowTearing),
                Windowed = true,
                BufferDesc = new(1, 0)
                {
                    Format = Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm,
                    Height = (uint)window.Height,
                    Width = (uint)window.Width,
                    Scaling = Silk.NET.DXGI.ModeScaling.Stretched,
                    ScanlineOrdering = Silk.NET.DXGI.ModeScanlineOrder.Unspecified
                },
                OutputWindow = Hwnd
            };

            IDXGISwapChain3* swapChain;
            IDXGIFactory->CreateSwapChain((IUnknown*)device.CommandQueue.Handle, &desc, (IDXGISwapChain**)&swapChain);
            IDXGIFactory->MakeWindowAssociation(Hwnd, 1 << 0);

            return new DXGISwapChain(device, swapChain, (int)desc.BufferDesc.Width, (int)desc.BufferDesc.Height, 2, (SwapChainFlag)desc.Flags);
        }

        private IDXGIAdapter1* GetHardwareAdapter()
        {
            IDXGIAdapter1* adapter = null;
            Guid* adapterGuid = Utils.Guid(IDXGIAdapter1.Guid);
            IDXGIFactory6* factory6;
            IDXGIFactory->QueryInterface(Utils.Guid(IDXGIFactory6.Guid), (void**)&factory6);

            if (factory6 != null)
            {
                for (uint adapterIndex = 0;
                    (ResultCode)factory6->EnumAdapterByGpuPreference(adapterIndex, GpuPreference.HighPerformance, adapterGuid, (void**)&adapter) !=
                    ResultCode.DXGI_ERROR_NOT_FOUND;
                    adapterIndex++)
                {
                    AdapterDesc1 desc;
                    adapter->GetDesc1(&desc);
                    if (((AdapterFlag)desc.Flags & AdapterFlag.Software) != AdapterFlag.None)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter->Release();
                        continue;
                    }

                    AdapterIndex = (int)adapterIndex;
                    return adapter;
                }

                factory6->Release();
            }

            if (adapter == null)
            {
                for (uint adapterIndex = 0;
                    (ResultCode)IDXGIFactory->EnumAdapters1(adapterIndex, &adapter) != ResultCode.DXGI_ERROR_NOT_FOUND;
                    adapterIndex++)
                {
                    AdapterDesc1 desc;
                    adapter->GetDesc1(&desc);
                    string name = new(desc.Description);

                    if (((AdapterFlag)desc.Flags & AdapterFlag.Software) != AdapterFlag.None)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter->Release();
                        continue;
                    }

                    AdapterIndex = (int)adapterIndex;
                    return adapter;
                }
            }

            return adapter;
        }

        public void PumpDebugMessages()
        {
            throw new NotImplementedException();
        }
    }
}