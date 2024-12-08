namespace HexaEngine.D3D11
{
    using Hexa.NET.D3D11On12;
    using Hexa.NET.D3D12;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    public unsafe partial class D3D11On12GraphicsDevice : D3D11GraphicsDevice
    {
        public new static readonly ShaderCompiler Compiler;

        public ComPtr<ID3D12Device> D3D12Device;

        static D3D11On12GraphicsDevice()
        {
            Compiler = new();
        }

        [SupportedOSPlatform("windows")]
        public D3D11On12GraphicsDevice(DXGIAdapterD3D11 adapter, bool debug) : base(adapter)
        {
            FeatureLevel[] levelsArr = new FeatureLevel[]
            {
                FeatureLevel.Level122,
                FeatureLevel.Level121,
                FeatureLevel.Level120,
                FeatureLevel.Level111,
                FeatureLevel.Level110
            };

            CreateDeviceFlag flags = CreateDeviceFlag.BgraSupport;

            if (debug)
            {
                flags |= CreateDeviceFlag.Debug;
            }

            ID3D11Device* tempDevice;
            ID3D11DeviceContext* tempContext;

            FeatureLevel level = 0;
            FeatureLevel* levels = (FeatureLevel*)Unsafe.AsPointer(ref levelsArr[0]);

            D3D12.CreateDevice(adapter.IDXGIAdapter.As<IUnknown>(), FeatureLevel.Level110, out D3D12Device).ThrowIf();
            D3D11On12.CreateDevice(D3D12Device.As<IUnknown>(), (uint)flags, levels, (uint)levelsArr.Length, (IUnknown**)null, 0, 0, &tempDevice, &tempContext, &level).ThrowIf();

            Level = level;

            tempDevice->QueryInterface(out Device);
            tempContext->QueryInterface(out DeviceContext);
            tempDevice->Release();
            tempContext->Release();

            NativePointer = new(Device.Handle);

#if DEBUG
            if (debug)
            {
                Device.QueryInterface(out Debug);
            }
#endif

            Context = new D3D11GraphicsContext(this);
        }

        public override GraphicsBackend Backend => GraphicsBackend.D3D11On12;

        public new event EventHandler? OnDisposed;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                OnDisposed?.Invoke(this, EventArgs.Empty);

                Context.Dispose();
                Device.Release();
                D3D12Device.Release();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

                if (Debug.Handle != null)
                {
                    Debug.ReportLiveDeviceObjects(Hexa.NET.D3D11.RldoFlags.Detail | Hexa.NET.D3D11.RldoFlags.IgnoreInternal);
                    Debug.Release();
                }

                LeakTracer.ReportLiveInstances();

                disposedValue = true;
            }
        }
    }
}