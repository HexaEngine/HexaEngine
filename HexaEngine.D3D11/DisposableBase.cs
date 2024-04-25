namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Debugging;
    using System;

    public abstract class DisposableBase : IDisposable
    {
        private bool disposedValue;
        private int refCounter;

        public DisposableBase()
        {
            refCounter = 1;
            LeakTracer.Allocate(this);
        }

        public bool IsDisposed => disposedValue;

        public event EventHandler? OnDisposed;

        /// <summary>
        /// Increases the reference count of this object.
        /// </summary>
        public void AddRef()
        {
            Interlocked.Increment(ref refCounter);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                var val = Interlocked.Decrement(ref refCounter);
                if (val > 0)
                {
                    return;
                }

                DisposeCore();
                OnDisposed?.Invoke(this, EventArgs.Empty);
                LeakTracer.Release(this);
                disposedValue = true;
            }
        }

        protected abstract void DisposeCore();

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}