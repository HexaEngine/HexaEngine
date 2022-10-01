namespace HexaEngine.D3D11
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.DXGI;
    using System.Diagnostics;
    using System.Runtime.Versioning;

    public unsafe class DXGIAdapter : IAdapter
    {
        internal readonly DXGI DXGI;

        internal IDXGIFactory2* IDXGIFactory;
        internal IDXGIAdapter1* IDXGIAdapter;

        public DXGIAdapter()
        {
            DXGI = DXGI.GetApi();

            IDXGIFactory2* factory;
            DXGI.CreateDXGIFactory2(0, Utils.Guid(IDXGIFactory2.Guid), (void**)&factory);
            IDXGIFactory = factory;

            IDXGIAdapter = GetHardwareAdapter();
        }

        public RenderBackend Backend => RenderBackend.D3D11;

        [SupportedOSPlatform("windows")]
        public IGraphicsDevice CreateGraphics(SdlWindow window)
        {
            return new D3D11GraphicsDevice(this, window);
        }

        [SupportedOSPlatform("windows")]
        internal ISwapChain CreateSwapChainForWindow(D3D11GraphicsDevice device, SdlWindow window)
        {
            SwapChainDesc1 desc = new()
            {
                Width = (uint)window.Width,
                Height = (uint)window.Height,
                Format = Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm,
                BufferCount = 2,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SampleDesc = new(1, 0),
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipSequential,
                Flags = (uint)(SwapChainFlag.AllowModeSwitch | SwapChainFlag.AllowTearing)
            };

            SwapChainFullscreenDesc fullscreenDesc = new()
            {
                Windowed = 1,
                RefreshRate = new Rational(0, 1),
                Scaling = ModeScaling.Unspecified,
                ScanlineOrdering = ModeScanlineOrder.Unspecified,
            };

            IDXGISwapChain1* swapChain;
            IntPtr hwnd = window.GetHWND();
            IDXGIFactory->CreateSwapChainForHwnd((IUnknown*)device.Device, hwnd, &desc, &fullscreenDesc, null, &swapChain);
            IDXGIFactory->MakeWindowAssociation(hwnd, 1 << 0);

            return new DXGISwapChain(device, swapChain, (SwapChainFlag)desc.Flags);
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
                    string name = new(desc.Description);

                    Trace.WriteLine($"Found Adapter {name}");

                    if (((AdapterFlag)desc.Flags & AdapterFlag.Software) != AdapterFlag.None)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter->Release();
                        continue;
                    }

                    Trace.WriteLine($"Using {name}");

                    return adapter;
                }

                factory6->Release();
            }

            if (adapter == null)
                for (uint adapterIndex = 0;
                    (ResultCode)IDXGIFactory->EnumAdapters1(adapterIndex, &adapter) != ResultCode.DXGI_ERROR_NOT_FOUND;
                    adapterIndex++)
                {
                    AdapterDesc1 desc;
                    adapter->GetDesc1(&desc);
                    string name = new(desc.Description);

                    Trace.WriteLine($"Found Adapter {name}");

                    if (((AdapterFlag)desc.Flags & AdapterFlag.Software) != AdapterFlag.None)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter->Release();
                        continue;
                    }

                    return adapter;
                }

            return adapter;
        }
    }
}