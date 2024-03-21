namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Core;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;

    public interface IUIRevivableResource : IUIResource
    {
        public void Revive();
    }

    public abstract class UIResource : IUIResource
    {
        private bool disposedValue;
        private long refCounter = 1;

        public void AddRef()
        {
            Interlocked.Increment(ref refCounter);
        }

        long IUIResource.ReferenceCount => Interlocked.Read(ref refCounter);

        public bool IsDisposed => disposedValue;

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (Interlocked.Decrement(ref refCounter) > 0)
                {
                    return;
                }

                DisposeCore();

                disposedValue = true;
            }
        }

        ~UIResource()
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

    public abstract class UIRevivableResource : IUIRevivableResource
    {
        private bool disposedValue;
        private long refCounter = 1;

        public void AddRef()
        {
            Interlocked.Increment(ref refCounter);
        }

        long IUIResource.ReferenceCount => Interlocked.Read(ref refCounter);

        public bool IsDisposed => disposedValue;

        protected abstract void ReviveCore();

        public void Revive()
        {
            if (!disposedValue)
            {
                return;
            }
            lock (this)
            {
                long refCount = Interlocked.Increment(ref refCounter);
                if (refCount > 1)
                {
                    // Another thread has already revived this instance.
                    return;
                }
                disposedValue = false;

                ReviveCore();
            }
        }

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (Interlocked.Decrement(ref refCounter) > 0)
                {
                    return;
                }

                DisposeCore();

                disposedValue = true;
            }
        }

        ~UIRevivableResource()
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

    public static class UIFactory
    {
        private static readonly List<IDisposable> resources = [];
        private static readonly object _lock = new();

        public static void DisposeResources()
        {
            for (int i = 0; i < resources.Count; i++)
            {
                resources[i].Dispose();
            }
        }
    }
}