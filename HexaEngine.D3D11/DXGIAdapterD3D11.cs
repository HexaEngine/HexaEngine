namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;

    public unsafe class DXGIAdapterD3D11 : IGraphicsAdapter
    {
        internal readonly DXGI DXGI;
        private readonly bool debug;

        internal ComPtr<IDXGIFactory2> IDXGIFactory;
        internal ComPtr<IDXGIAdapter1> IDXGIAdapter;
        internal ComPtr<IDXGIAdapter3> IDXGIAdapter3;
        internal ComPtr<IDXGIDebug> IDXGIDebug;
        internal ComPtr<IDXGIInfoQueue> IDXGIInfoQueue;

        private readonly Guid DXGI_DEBUG_ALL = new(0xe48ae283, 0xda80, 0x490b, 0x87, 0xe6, 0x43, 0xe9, 0xa9, 0xcf, 0xda, 0x8);
        private readonly Guid DXGI_DEBUG_DX = new(0x35cdd7fc, 0x13b2, 0x421d, 0xa5, 0xd7, 0x7e, 0x44, 0x51, 0x28, 0x7d, 0x64);
        private readonly Guid DXGI_DEBUG_DXGI = new(0x25cddaa4, 0xb1c6, 0x47e1, 0xac, 0x3e, 0x98, 0x87, 0x5b, 0x5a, 0x2e, 0x2a);
        private readonly Guid DXGI_DEBUG_APP = new(0x6cd6e01, 0x4219, 0x4ebd, 0x87, 0x9, 0x27, 0xed, 0x23, 0x36, 0xc, 0x62);
        private readonly Guid DXGI_DEBUG_D3D11 = new(0x4b99317b, 0xac39, 0x4aa6, 0xbb, 0xb, 0xba, 0xa0, 0x47, 0x84, 0x79, 0x8f);

        public DXGIAdapterD3D11(bool debug)
        {
            DXGI = DXGI.GetApi();
            if (debug)
            {
                DXGI.GetDebugInterface1(0, out IDXGIDebug);
                DXGI.GetDebugInterface1(0, out IDXGIInfoQueue);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Message, false);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Info, false);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Warning, true);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Error, true);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Corruption, true);
            }

            DXGI.CreateDXGIFactory2(debug ? 0x01u : 0x00u, out IDXGIFactory);

            IDXGIAdapter = GetHardwareAdapter();
            IDXGIAdapter.QueryInterface(out IDXGIAdapter3);
            this.debug = debug;
        }

        public static void Init(bool debug)
        {
            if (OperatingSystem.IsWindows())
            {
                GraphicsAdapter.Adapters.Add(new DXGIAdapterD3D11(debug));
            }
        }

        public static string Convert(InfoQueueMessageSeverity severity)
        {
            return severity switch
            {
                InfoQueueMessageSeverity.Corruption => "CORRUPTION",
                InfoQueueMessageSeverity.Error => "ERROR",
                InfoQueueMessageSeverity.Warning => "WARNING",
                InfoQueueMessageSeverity.Info => "INFO",
                InfoQueueMessageSeverity.Message => "LOG",
                _ => throw new NotImplementedException(),
            };
        }

        public static string Convert(InfoQueueMessageCategory category)
        {
            return category switch
            {
                InfoQueueMessageCategory.Unknown => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_UNKNOWN",
                InfoQueueMessageCategory.Miscellaneous => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_MISCELLANEOUS",
                InfoQueueMessageCategory.Initialization => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_INITIALIZATION",
                InfoQueueMessageCategory.Cleanup => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_CLEANUP",
                InfoQueueMessageCategory.Compilation => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_COMPILATION",
                InfoQueueMessageCategory.StateCreation => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_CREATION",
                InfoQueueMessageCategory.StateSetting => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_SETTING",
                InfoQueueMessageCategory.StateGetting => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_GETTING",
                InfoQueueMessageCategory.ResourceManipulation => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_RESOURCE_MANIPULATION",
                InfoQueueMessageCategory.Execution => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_EXECUTION",
                InfoQueueMessageCategory.Shader => "DXGI_INFO_QUEUE_MESSAGE_CATEGORY_SHADER",
                _ => throw new NotImplementedException(),
            };
        }

        public void PumpDebugMessages()
        {
            ulong messageCount = IDXGIInfoQueue.GetNumStoredMessages(DXGI_DEBUG_ALL);
            for (ulong i = 0; i < messageCount; i++)
            {
                nuint messageLength;

                HResult hr = IDXGIInfoQueue.GetMessageA(DXGI_DEBUG_ALL, i, (InfoQueueMessage*)null, &messageLength);

                if (hr.IsSuccess)
                {
                    InfoQueueMessage* message = (InfoQueueMessage*)Malloc(messageLength);

                    hr = IDXGIInfoQueue.GetMessageA(DXGI_DEBUG_ALL, i, message, &messageLength);

                    if (hr.IsSuccess)
                    {
                        string msg = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(message->PDescription));

                        if (message->Producer == DXGI_DEBUG_DX)
                            ImGuiConsole.Log($"DX {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        if (message->Producer == DXGI_DEBUG_DXGI)
                            ImGuiConsole.Log($"DXGI {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        if (message->Producer == DXGI_DEBUG_APP)
                            ImGuiConsole.Log($"APP {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        if (message->Producer == DXGI_DEBUG_D3D11)
                            ImGuiConsole.Log($"D3D11 {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");

                        Free(message);
                    }
                }
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

        public virtual IGraphicsDevice CreateGraphicsDevice(bool debug)
        {
            AdapterDesc1 desc;
            IDXGIAdapter.GetDesc1(&desc);
            string name = new(desc.Description);

            ImGuiConsole.Log(LogSeverity.Info, "Backend: Using Graphics API: D3D11");
            ImGuiConsole.Log(LogSeverity.Info, $"Backend: Using Graphics Device: {name}");
            return new D3D11GraphicsDevice(this, debug);
        }

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

            ComPtr<IDXGISwapChain1> swapChain;
            IntPtr hwnd = window.GetWin32HWND();
            IDXGIFactory.CreateSwapChainForHwnd((IUnknown*)device.Device.Handle, hwnd, &desc, &fullscreenDesc, (IDXGIOutput*)null, &swapChain.Handle);
            IDXGIFactory.MakeWindowAssociation(hwnd, 1 << 0);

            return new DXGISwapChain(device, swapChain, (SwapChainFlag)desc.Flags);
        }

        private IDXGIAdapter1* GetHardwareAdapter()
        {
            IDXGIAdapter1* adapter = null;
            Guid* adapterGuid = Utils.Guid(IDXGIAdapter1.Guid);
            IDXGIFactory6* factory6;
            IDXGIFactory.QueryInterface(Utils.Guid(IDXGIFactory6.Guid), (void**)&factory6);

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

                    return adapter;
                }

                factory6->Release();
            }

            if (adapter == null)
            {
                for (uint adapterIndex = 0;
                    (ResultCode)IDXGIFactory.EnumAdapters1(adapterIndex, &adapter) != ResultCode.DXGI_ERROR_NOT_FOUND;
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
            }

            return adapter;
        }
    }
}