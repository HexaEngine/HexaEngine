namespace HexaEngine.Dummy
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using System;

    public unsafe class DummyObject : DisposableBase, IDeviceChild
    {
        private readonly void* dummy;

        public DummyObject()
        {
            dummy = Utils.Alloc(sizeof(nint));
            NativePointer = (nint)dummy;
        }

        public string? DebugName { get; set; }

        public nint NativePointer { get; }

        protected override void DisposeCore()
        {
            Utils.Free(dummy);
        }
    }

    public abstract class DisposableBase : IDisposable
    {
        private bool disposedValue;

        public bool IsDisposed => disposedValue;

        public event EventHandler? OnDisposed;

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                OnDisposed?.Invoke(this, EventArgs.Empty);
                disposedValue = true;
            }
        }

        ~DisposableBase()
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