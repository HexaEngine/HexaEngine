namespace HexaEngine.Core
{
    using HexaEngine.Core.Debugging;

    public abstract class DisposableBase : IDisposable
    {
        private bool disposedValue;

        public DisposableBase()
        {
            LeakTracer.Allocate(this);
        }

        public bool IsDisposed => disposedValue;

        public event EventHandler? OnDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                OnDisposed?.Invoke(this, EventArgs.Empty);
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

    public interface IDisposableRef : IDisposable
    {
        void AddRef();
    }

    public abstract class DisposableRefBase : IDisposableRef
    {
        private bool disposedValue;
        private long counter;

        public DisposableRefBase()
        {
            LeakTracer.Allocate(this);
        }

        public bool IsDisposed => disposedValue;

        public event EventHandler? OnDisposed;

        public void AddRef()
        {
            Interlocked.Increment(ref counter);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Decrement(ref counter) != 0)
            {
                return;
            }

            if (!disposedValue)
            {
                DisposeCore();
                OnDisposed?.Invoke(this, EventArgs.Empty);
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