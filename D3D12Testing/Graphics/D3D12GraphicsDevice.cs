namespace D3D12Testing.Graphics
{
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D12;
    using System;

    public unsafe class D3D12GraphicsDevice : IDisposable
    {
        internal static readonly D3D12 D3D12 = D3D12.GetApi();
        internal readonly ComPtr<ID3D12Device10> Device;
        internal ComPtr<ID3D12CommandAllocator> CommandAllocator;
        internal ComPtr<ID3D12CommandQueue> CommandQueue;
        internal ComPtr<ID3D12Fence> Fence;
        internal ComPtr<ID3D12Debug> Debug;

        private bool disposedValue;

        public D3D12GraphicsDevice(DXGIAdapterD3D12 adapter, bool debug)
        {
            if (debug)
            {
                D3D12.GetDebugInterface(out Debug).ThrowHResult();
                Debug.EnableDebugLayer();
            }

            D3D12.CreateDevice(adapter.IDXGIAdapter, D3DFeatureLevel.Level122, out Device).ThrowHResult();

            CommandQueueDesc commandQueueDesc = new()
            {
                Type = CommandListType.Direct,
                Flags = CommandQueueFlags.None,
            };

            Device.CreateCommandQueue(&commandQueueDesc, out CommandQueue).ThrowHResult();

            Device.CreateCommandAllocator(CommandListType.Direct, out CommandAllocator).ThrowHResult();

            Device.CreateCommandList(0, CommandListType.Direct, CommandAllocator, new ComPtr<ID3D12PipelineState>(), out ComPtr<ID3D12GraphicsCommandList> list).ThrowHResult();
        }

        public string? DebugName { get; set; }

        public bool IsDisposed => disposedValue;

        public nint NativePointer => (nint)Device.Handle;

        public event EventHandler? OnDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Device.Release();

                OnDisposed?.Invoke(this, EventArgs.Empty);
                disposedValue = true;
            }
        }

        ~D3D12GraphicsDevice()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}