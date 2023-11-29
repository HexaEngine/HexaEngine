namespace D3D12Testing.Graphics
{
    using D3D12Testing.Windows;
    using Silk.NET.Core.Contexts;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D12;
    using Silk.NET.DXGI;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;
    using InfoQueueFilter = Silk.NET.DXGI.InfoQueueFilter;

    public unsafe class DXGIAdapterD3D12
    {
        internal readonly DXGI DXGI;
        internal readonly INativeWindowSource source;
        private readonly bool debug;

        internal ComPtr<IDXGIFactory7> IDXGIFactory;
        internal ComPtr<IDXGIAdapter4> IDXGIAdapter;
        internal ComPtr<IDXGIDebug> IDXGIDebug;

        internal ComPtr<IDXGIInfoQueue> IDXGIInfoQueue;

        private readonly Guid DXGI_DEBUG_ALL = new(0xe48ae283, 0xda80, 0x490b, 0x87, 0xe6, 0x43, 0xe9, 0xa9, 0xcf, 0xda, 0x8);
        private readonly Guid DXGI_DEBUG_DX = new(0x35cdd7fc, 0x13b2, 0x421d, 0xa5, 0xd7, 0x7e, 0x44, 0x51, 0x28, 0x7d, 0x64);
        private readonly Guid DXGI_DEBUG_DXGI = new(0x25cddaa4, 0xb1c6, 0x47e1, 0xac, 0x3e, 0x98, 0x87, 0x5b, 0x5a, 0x2e, 0x2a);
        private readonly Guid DXGI_DEBUG_APP = new(0x6cd6e01, 0x4219, 0x4ebd, 0x87, 0x9, 0x27, 0xed, 0x23, 0x36, 0xc, 0x62);
        private readonly Guid DXGI_DEBUG_D3D11 = new(0x4b99317b, 0xac39, 0x4aa6, 0xbb, 0xb, 0xba, 0xa0, 0x47, 0x84, 0x79, 0x8f);

        public DXGIAdapterD3D12(INativeWindowSource source, bool debug)
        {
            DXGI = DXGI.GetApi(source);
            if (debug)
            {
                DXGI.GetDebugInterface1(0, out IDXGIDebug);
                DXGI.GetDebugInterface1(0, out IDXGIInfoQueue);

                InfoQueueFilter filter = new();
                filter.DenyList.NumIDs = 1;
                filter.DenyList.PIDList = (int*)AllocT(MessageID.SetprivatedataChangingparams);
                IDXGIInfoQueue.AddStorageFilterEntries(DXGI_DEBUG_ALL, &filter);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Message, false);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Info, false);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Warning, true);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Error, true);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Corruption, true);
                Free(filter.DenyList.PIDList);
            }

            DXGI.CreateDXGIFactory2(debug ? 0x01u : 0x00u, out IDXGIFactory);

            IDXGIAdapter = GetHardwareAdapter();
            this.source = source;
            this.debug = debug;
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
            if (!debug)
                return;
            ulong messageCount = IDXGIInfoQueue.GetNumStoredMessages(DXGI_DEBUG_ALL);
            for (ulong i = 0; i < messageCount; i++)
            {
                nuint messageLength;

                HResult hr = IDXGIInfoQueue.GetMessageA(DXGI_DEBUG_ALL, i, (InfoQueueMessage*)null, &messageLength);

                if (hr.IsSuccess)
                {
                    InfoQueueMessage* message = (InfoQueueMessage*)Alloc(messageLength);

                    hr = IDXGIInfoQueue.GetMessageA(DXGI_DEBUG_ALL, i, message, &messageLength);

                    if (hr.IsSuccess)
                    {
                        string msg = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(message->PDescription));

                        if (message->Producer == DXGI_DEBUG_DX)
                            Trace.WriteLine($"DX {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        if (message->Producer == DXGI_DEBUG_DXGI)
                            Trace.WriteLine($"DXGI {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        if (message->Producer == DXGI_DEBUG_APP)
                            Trace.WriteLine($"APP {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        if (message->Producer == DXGI_DEBUG_D3D11)
                            Trace.WriteLine($"D3D11 {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");

                        Free(message);
                    }
                }
            }

            IDXGIInfoQueue.ClearStoredMessages(DXGI_DEBUG_ALL);
        }

        public ulong GetMemoryBudget()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.Budget;
        }

        public ulong GetMemoryCurrentUsage()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.CurrentUsage;
        }

        public ulong GetMemoryAvailableForReservation()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.AvailableForReservation;
        }

        public ulong GetMemoryCurrentReservation()
        {
            QueryVideoMemoryInfo memoryInfo;
            IDXGIAdapter.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local, &memoryInfo);
            return memoryInfo.AvailableForReservation;
        }

        public virtual int PlatformScore => 100;

        [SupportedOSPlatform("windows")]
        public virtual D3D12GraphicsDevice CreateGraphicsDevice(bool debug)
        {
            AdapterDesc1 desc;
            IDXGIAdapter.GetDesc1(&desc);
            string name = new(desc.Description);

            Trace.WriteLine("Backend: Using Graphics API: D3D12");
            Trace.WriteLine($"Backend: Using Graphics Device: {name}");
            return new D3D12GraphicsDevice(this, debug);
        }

        [SupportedOSPlatform("windows")]
        internal DXGISwapChain CreateSwapChainForWindow(D3D12GraphicsDevice device, SdlWindow window)
        {
            var (Hwnd, HDC, HInstance) = window.Win32 ?? throw new NotSupportedException();

            SwapChainDesc desc = new()
            {
                BufferCount = 2,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SampleDesc = new(1, 0),
                SwapEffect = SwapEffect.FlipSequential,
                Flags = (uint)(SwapChainFlag.AllowModeSwitch | SwapChainFlag.AllowTearing),
                Windowed = true,
                BufferDesc = new(1, 0)
                {
                    Format = Format.FormatB8G8R8A8Unorm,
                    Height = (uint)window.Height,
                    Width = (uint)window.Width,
                    Scaling = ModeScaling.Stretched,
                    ScanlineOrdering = ModeScanlineOrder.Unspecified
                },
                OutputWindow = Hwnd
            };

            IDXGISwapChain3* swapChain;
            IDXGIFactory.CreateSwapChain((IUnknown*)device.CommandQueue.Handle, &desc, (IDXGISwapChain**)&swapChain);
            IDXGIFactory.MakeWindowAssociation(Hwnd, 1 << 0);

            return new DXGISwapChain(device, swapChain, (int)desc.BufferDesc.Width, (int)desc.BufferDesc.Height, 2, (SwapChainFlag)desc.Flags);
        }

        private ComPtr<IDXGIAdapter4> GetHardwareAdapter()
        {
            ComPtr<IDXGIAdapter4> selected = null;
            for (uint adapterIndex = 0;
                (ResultCode)IDXGIFactory.EnumAdapterByGpuPreference(adapterIndex, GpuPreference.HighPerformance, out ComPtr<IDXGIAdapter4> adapter) !=
                ResultCode.DXGI_ERROR_NOT_FOUND;
                adapterIndex++)
            {
                AdapterDesc1 desc;
                adapter.GetDesc1(&desc);
                //gpus.Add(new(new(desc.Description), desc.VendorId, desc.DeviceId, desc.SubSysId, desc.Revision, desc.DedicatedVideoMemory, desc.DedicatedSystemMemory, desc.SharedSystemMemory, desc.AdapterLuid, desc.Flags));

                if (((AdapterFlag)desc.Flags & AdapterFlag.Software) != AdapterFlag.None)
                {
                    // Don't select the Basic Render Driver adapter.
                    adapter.Release();
                    continue;
                }

                selected = adapter;
            }

            if (selected.Handle == null)
                throw new NotSupportedException();
            return selected;
        }
    }
}