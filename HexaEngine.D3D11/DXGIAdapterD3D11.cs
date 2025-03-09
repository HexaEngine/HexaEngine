namespace HexaEngine.D3D11
{
    using Hexa.NET.Logging;
    using Hexa.NET.SDL3;
    using HexaEngine.Core.Debugging.Device;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaGen.Runtime.COM;
    using Silk.NET.Core.Contexts;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using InfoQueueFilter = Hexa.NET.DXGI.InfoQueueFilter;

    public unsafe class DXGIAdapterD3D11 : IGraphicsAdapter, IDisposable
    {
        internal readonly List<GPU> gpus = new();
        private readonly bool debug;

        internal ComPtr<IDXGIFactory7> IDXGIFactory;
        internal ComPtr<IDXGIAdapter4> IDXGIAdapter;
        internal ComPtr<IDXGIOutput6> IDXGIOutput;
        internal ComPtr<IDXGIDebug> IDXGIDebug;

        internal ComPtr<IDXGIInfoQueue> IDXGIInfoQueue;

        private bool disposedValue;

        private readonly Guid DXGI_DEBUG_ALL = new(0xe48ae283, 0xda80, 0x490b, 0x87, 0xe6, 0x43, 0xe9, 0xa9, 0xcf, 0xda, 0x8);
        private readonly Guid DXGI_DEBUG_DX = new(0x35cdd7fc, 0x13b2, 0x421d, 0xa5, 0xd7, 0x7e, 0x44, 0x51, 0x28, 0x7d, 0x64);
        private readonly Guid DXGI_DEBUG_DXGI = new(0x25cddaa4, 0xb1c6, 0x47e1, 0xac, 0x3e, 0x98, 0x87, 0x5b, 0x5a, 0x2e, 0x2a);
        private readonly Guid DXGI_DEBUG_APP = new(0x6cd6e01, 0x4219, 0x4ebd, 0x87, 0x9, 0x27, 0xed, 0x23, 0x36, 0xc, 0x62);
        private readonly Guid DXGI_DEBUG_D3D11 = new(0x4b99317b, 0xac39, 0x4aa6, 0xbb, 0xb, 0xba, 0xa0, 0x47, 0x84, 0x79, 0x8f);

        public DXGIAdapterD3D11(INativeWindowSource source, bool debug)
        {
            if (debug)
            {
                DXGI.GetDebugInterface1(0, out IDXGIDebug);
                DXGI.GetDebugInterface1(0, out IDXGIInfoQueue);

                InfoQueueFilter filter = new();
                filter.DenyList.NumIDs = 1;
                filter.DenyList.PIDList = (int*)AllocT(MessageId.SetprivatedataChangingparams);
                IDXGIInfoQueue.AddStorageFilterEntries(DXGI_DEBUG_ALL, &filter);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Message, false);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Info, false);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Warning, true);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Error, true);
                IDXGIInfoQueue.SetBreakOnSeverity(DXGI_DEBUG_ALL, InfoQueueMessageSeverity.Corruption, true);
                Free(filter.DenyList.PIDList);
            }

            DXGI.CreateDXGIFactory2(debug ? 0x01u : 0x00u, out IDXGIFactory);

            IDXGIAdapter = GetHardwareAdapter(null);
            IDXGIOutput = GetOutput(null);
            this.debug = debug;
        }

        public static void Init(INativeWindowSource source, bool debug)
        {
            GraphicsAdapter.Adapters.Add(new DXGIAdapterD3D11(source, debug));
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

        private static readonly ILogger D3D11Logger = LoggerFactory.GetLogger(nameof(D3D11));
        private static readonly ILogger DXGILogger = LoggerFactory.GetLogger(nameof(DXGI));

        public void PumpDebugMessages()
        {
            if (!debug)
            {
                return;
            }

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
                        {
                            D3D11Logger.Log($"DX {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        }

                        if (message->Producer == DXGI_DEBUG_DXGI)
                        {
                            DXGILogger.Log($"DXGI {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        }

                        if (message->Producer == DXGI_DEBUG_APP)
                        {
                            D3D11Logger.Log($"APP {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        }

                        if (message->Producer == DXGI_DEBUG_D3D11)
                        {
                            D3D11Logger.Log($"D3D11 {Convert(message->Severity)}: {msg} [ {Convert(message->Category)} ]");
                        }

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

        public virtual GraphicsBackend Backend => GraphicsBackend.D3D11;

        public virtual int PlatformScore => 100;

        public virtual IReadOnlyList<GPU> GPUs => gpus;

        public int AdapterIndex { get; private set; } = -1;

        public virtual IGraphicsDevice CreateGraphicsDevice(bool debug)
        {
            AdapterDesc1 desc;
            IDXGIAdapter.GetDesc1(&desc);
            string name = new(&desc.Description_0);

            LoggerFactory.General.Info("Backend: Using Graphics API: D3D11");
            LoggerFactory.General.Info($"Backend: Using Graphics Device: {name}");

            return new D3D11GraphicsDevice(this, debug);
        }

        internal ISwapChain CreateSwapChainForWindow(D3D11GraphicsDevice device, SdlWindow window)
        {
            var hwnd = window.GetHWND();

            SwapChainDesc1 desc = new()
            {
                Width = (uint)window.Width,
                Height = (uint)window.Height,
                Format = AutoChooseSwapChainFormat(device.Device, IDXGIOutput, out var colorSpace),
                BufferCount = 2,
                BufferUsage = (uint)DXGI.DXGI_USAGE_RENDER_TARGET_OUTPUT,
                SampleDesc = new(1, 0),
                Scaling = Hexa.NET.DXGI.Scaling.Stretch,
                SwapEffect = Hexa.NET.DXGI.SwapEffect.FlipSequential,
                Flags = (uint)(SwapChainFlag.AllowModeSwitch | SwapChainFlag.AllowTearing),
                Stereo = false,
            };

            SwapChainFullscreenDesc fullscreenDesc = new()
            {
                Windowed = 1,
                RefreshRate = new Rational(0, 1),
                Scaling = Hexa.NET.DXGI.ModeScaling.Unspecified,
                ScanlineOrdering = Hexa.NET.DXGI.ModeScanlineOrder.Unspecified,
            };

            IDXGIFactory.CreateSwapChainForHwnd(device.Device.As<IUnknown>(), hwnd, &desc, &fullscreenDesc, IDXGIOutput.As<IDXGIOutput>(), out ComPtr<IDXGISwapChain1> swapChain);

            swapChain.QueryInterface<IDXGISwapChain3>(out var swapChain3);
            swapChain.Release();

            return new DXGISwapChain(device, swapChain3, (SwapChainFlag)desc.Flags);
        }

        internal ISwapChain CreateSwapChainForWindow(D3D11GraphicsDevice device, SdlWindow window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            var (Hwnd, HDC, HInstance) = window.Win32 ?? throw new NotSupportedException();

            SwapChainDesc1 desc = Helper.Convert(swapChainDescription);

            SwapChainFullscreenDesc fullscreenDesc = Helper.Convert(fullscreenDescription);

            IDXGIFactory.CreateSwapChainForHwnd(device.Device.As<IUnknown>(), Hwnd, &desc, &fullscreenDesc, IDXGIOutput.As<IDXGIOutput>(), out ComPtr<IDXGISwapChain1> swapChain);

            swapChain.QueryInterface<IDXGISwapChain3>(out var swapChain3);
            swapChain.Release();

            return new DXGISwapChain(device, swapChain3, (SwapChainFlag)desc.Flags);
        }

        internal ISwapChain CreateSwapChainForWindow(D3D11GraphicsDevice device, SDLWindow* window)
        {
            uint props = SDL.GetWindowProperties(window);
            var hwnd = (nint)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_WIN32_HWND_POINTER, null);

            int width = 0;
            int height = 0;

            SDL.GetWindowSize(window, &width, &height);

            SwapChainDesc1 desc = new()
            {
                Width = (uint)width,
                Height = (uint)height,
                Format = AutoChooseSwapChainFormat(device.Device, IDXGIOutput, out var colorSpace),
                BufferCount = 2,
                BufferUsage = (uint)DXGI.DXGI_USAGE_RENDER_TARGET_OUTPUT,
                SampleDesc = new(1, 0),
                Scaling = Hexa.NET.DXGI.Scaling.Stretch,
                SwapEffect = Hexa.NET.DXGI.SwapEffect.FlipSequential,
                Flags = (uint)(SwapChainFlag.AllowModeSwitch | SwapChainFlag.AllowTearing)
            };

            SwapChainFullscreenDesc fullscreenDesc = new()
            {
                Windowed = true,
                RefreshRate = new Rational(0, 1),
                Scaling = Hexa.NET.DXGI.ModeScaling.Unspecified,
                ScanlineOrdering = Hexa.NET.DXGI.ModeScanlineOrder.Unspecified,
            };

            IDXGIFactory.CreateSwapChainForHwnd(device.Device.As<IUnknown>(), hwnd, &desc, &fullscreenDesc, IDXGIOutput.As<IDXGIOutput>(), out ComPtr<IDXGISwapChain1> swapChain);

            swapChain.QueryInterface<IDXGISwapChain3>(out var swapChain3);
            swapChain.Release();

            return new DXGISwapChain(device, swapChain3, (SwapChainFlag)desc.Flags);
        }

        internal ISwapChain CreateSwapChainForWindow(D3D11GraphicsDevice device, SDLWindow* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            uint props = SDL.GetWindowProperties(window);
            var hwnd = (nint)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_WIN32_HWND_POINTER, null);

            SwapChainDesc1 desc = Helper.Convert(swapChainDescription);
            desc.Format = ChooseSwapChainFormat(device.Device, desc.Format);

            SwapChainFullscreenDesc fullscreenDesc = Helper.Convert(fullscreenDescription);

            IDXGIFactory.CreateSwapChainForHwnd(device.Device.As<IUnknown>(), hwnd, &desc, &fullscreenDesc, IDXGIOutput.As<IDXGIOutput>(), out ComPtr<IDXGISwapChain1> swapChain);

            swapChain.QueryInterface<IDXGISwapChain3>(out var swapChain3);
            swapChain.Release();

            return new DXGISwapChain(device, swapChain3, (SwapChainFlag)desc.Flags);
        }

        private ComPtr<IDXGIAdapter4> GetHardwareAdapter(string? name)
        {
            ComPtr<IDXGIAdapter4> selected = null;
            for (uint adapterIndex = 0;
                (ResultCode)IDXGIFactory.EnumAdapterByGpuPreference(adapterIndex, GpuPreference.HighPerformance, out ComPtr<IDXGIAdapter4> adapter).Value !=
                ResultCode.DXGI_ERROR_NOT_FOUND;
                adapterIndex++)
            {
                AdapterDesc1 desc;
                adapter.GetDesc1(&desc).ThrowIf();
                gpus.Add(new(new(&desc.Description_0), desc.VendorId, desc.DeviceId, desc.SubSysId, desc.Revision, desc.DedicatedVideoMemory, desc.DedicatedSystemMemory, desc.SharedSystemMemory, new(desc.AdapterLuid.LowPart, desc.AdapterLuid.HighPart), desc.Flags));

                var nameSpan = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(&desc.Description_0);

                // select by adapter name (description)
                if (name != null && nameSpan == name)
                {
                    AdapterIndex = (int)adapterIndex;
                    return adapter;
                }

                if (((AdapterFlag)desc.Flags & AdapterFlag.Software) != AdapterFlag.None)
                {
                    // Don't select the Basic Render Driver adapter.
                    adapter.Release();
                    continue;
                }

                if (AdapterIndex == -1)
                {
                    AdapterIndex = (int)adapterIndex;
                    selected = adapter;
                }
            }

            if (selected.Handle == null)
            {
                throw new NotSupportedException("No compatible GPU found. Please ensure your system meets the minimum requirements.");
            }

            return selected;
        }

        private ComPtr<IDXGIOutput6> GetOutput(string? name)
        {
            ComPtr<IDXGIOutput6> selected = null;
            ComPtr<IDXGIOutput> output = null;

            for (uint outputIndex = 0;
                (ResultCode)IDXGIAdapter.EnumOutputs(outputIndex, out output).Value !=
                ResultCode.DXGI_ERROR_NOT_FOUND;
                outputIndex++)
            {
                output.QueryInterface(out ComPtr<IDXGIOutput6> output6);

                OutputDesc1 desc;
                output6.GetDesc1(&desc).ThrowIf();

                // select the user chosen display by name.
                var nameSpan = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(&desc.DeviceName_0);
                if (name != null)
                {
                    if (nameSpan == name)
                    {
                        return output6;
                    }

                    output6.Release();
                    continue;
                }

                selected = output6;

                // select primary monitor.
                if (desc.DesktopCoordinates.Top == 0 && desc.DesktopCoordinates.Left == 0)
                {
                    break;
                }
            }

            if (selected.Handle == null)
            {
                throw new NotSupportedException("No output found. Please connect a monitor or make sure your monitors don't run over the iGPU if you are on Mobile/Laptop.");
            }

            return selected;
        }

        private static bool CheckSwapChainFormat(ComPtr<ID3D11Device5> device, Hexa.NET.DXGI.Format target)
        {
            FormatSupport formatSupport;
            device.CheckFormatSupport(target, (uint*)&formatSupport).ThrowIf();
            return formatSupport.HasFlag(FormatSupport.Display | FormatSupport.RenderTarget);
        }

        private static Hexa.NET.DXGI.Format ChooseSwapChainFormat(ComPtr<ID3D11Device5> device, Hexa.NET.DXGI.Format preferredFormat)
        {
            // Check if the preferred format is supported
            if (CheckSwapChainFormat(device, preferredFormat))
            {
                // Use the preferred format
                return preferredFormat;
            }
            else
            {
                // Fallback to B8G8R8A8_UNorm if the preferred format is not supported
                return Hexa.NET.DXGI.Format.B8G8R8A8Unorm;
            }
        }

        private static Hexa.NET.DXGI.Format AutoChooseSwapChainFormat(ComPtr<ID3D11Device5> device, ComPtr<IDXGIOutput6> output, out ColorSpaceType colorSpace)
        {
            colorSpace = ColorSpaceType.RgbFullG22NoneP709;
            if (output.Handle == null)
            {
                return Hexa.NET.DXGI.Format.B8G8R8A8Unorm;
            }

            OutputDesc1 desc;
            output.GetDesc1(&desc).ThrowIf();

            if (desc.ColorSpace == ColorSpaceType.RgbFullG2084NoneP2020)
            {
                return ChooseSwapChainFormat(device, Hexa.NET.DXGI.Format.R10G10B10A2Unorm);
            }

            if (desc.ColorSpace == ColorSpaceType.RgbFullG22NoneP709)
            {
                return ChooseSwapChainFormat(device, Hexa.NET.DXGI.Format.B8G8R8A8Unorm);
            }

            // If none of the preferred formats is supported, choose a fallback format
            return Hexa.NET.DXGI.Format.B8G8R8A8Unorm;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                IDXGIInfoQueue.Release();
                IDXGIDebug.Release();
                IDXGIOutput.Release();
                IDXGIAdapter.Release();
                IDXGIFactory.Release();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}