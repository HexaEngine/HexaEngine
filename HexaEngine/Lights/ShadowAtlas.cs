namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe class ShadowAtlas : IDisposable
    {
        private readonly string dbgName;
        private readonly int size;
        private readonly int layerCount;

        private readonly Mutex mutex = new();
        private readonly Texture2D texture;
        private readonly SpatialAllocator allocator;
        private readonly SpatialCache cache;

        private bool disposedValue;

        public ShadowAtlas(ShadowAtlasDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"ShadowAtlas: {Path.GetFileName(filename)}, Line: {lineNumber}";
            size = description.Size;
            layerCount = description.Layers;
            allocator = new(new(size), layerCount);
            cache = new(new(size), layerCount);
            texture = new(description.Format, description.Size, description.Size, description.ArraySize, 1, CpuAccessFlags.None, GpuAccessFlags.RW, filename: filename, lineNumber: lineNumber);
            texture.DebugName = dbgName;
        }

        public IRenderTargetView RTV => texture.RTV;

        public IShaderResourceView SRV => texture.SRV;

        public string DebugName => dbgName;

        public Viewport Viewport => texture.Viewport;

        public int LayerCount => layerCount;

        public float Size => size;

        public bool IsDisposed => disposedValue;

        public event Action<ShadowAtlas>? OnDisposing;

        public void Clear()
        {
            mutex.WaitOne();
            allocator.Clear();
            mutex.ReleaseMutex();
        }

        public ShadowAtlasHandle Alloc(int desiredSize, SpatialCacheHandle? cacheHandle = null)
        {
            return desiredSize <= 0 ? throw new ArgumentOutOfRangeException(nameof(desiredSize)) : Alloc((uint)desiredSize, cacheHandle);
        }

        public ShadowAtlasHandle Alloc(uint desiredSize, SpatialCacheHandle? cacheHandle = null)
        {
            Vector2 size = new(desiredSize);
            mutex.WaitOne();
            SpatialAllocatorHandle handle;
            if (cacheHandle != null && cacheHandle.Handle.Size == size && cache.TryRemoveFromCache(cacheHandle))
            {
                handle = cacheHandle.Handle;
            }
            else
            {
                handle = allocator.Alloc(size);
                handle ??= cache.AllocFromCache(size);
            }

            mutex.ReleaseMutex();

            return new(this, handle);
        }

        public ShadowAtlasHandle Alloc(Vector2 size, SpatialCacheHandle? cacheHandle = null)
        {
            mutex.WaitOne();
            SpatialAllocatorHandle handle;
            if (cacheHandle != null && cacheHandle.Handle.Size == size && cache.TryRemoveFromCache(cacheHandle))
            {
                handle = cacheHandle.Handle;
            }
            else
            {
                handle = allocator.Alloc(size);
                handle ??= cache.AllocFromCache(size);
            }

            mutex.ReleaseMutex();

            return new(this, handle);
        }

        public ShadowAtlasRangeHandle AllocRange(int desiredSize, int count, SpatialCacheHandle[]? cacheHandles = null)
        {
            return desiredSize <= 0 ? throw new ArgumentOutOfRangeException(nameof(desiredSize)) : AllocRange((uint)desiredSize, count, cacheHandles);
        }

        public ShadowAtlasRangeHandle AllocRange(uint desiredSize, int count, SpatialCacheHandle[]? cacheHandles = null)
        {
            SpatialAllocatorHandle[] handles = new SpatialAllocatorHandle[count];
            mutex.WaitOne();

            if (cacheHandles != null)
            {
                for (uint i = 0; i < cacheHandles.Length; i++)
                {
                    var cacheHandle = cacheHandles[i];
                    if (cache.TryRemoveFromCache(cacheHandle))
                    {
                        handles[i] = cacheHandle.Handle;
                    }
                    else
                    {
                        var handle = allocator.Alloc(new(desiredSize));
                        handle ??= cache.AllocFromCache(new(desiredSize));
                        handles[i] = handle;
                    }
                }
            }
            else
            {
                for (uint i = 0; i < count; i++)
                {
                    var handle = allocator.Alloc(new(desiredSize));
                    handle ??= cache.AllocFromCache(new(desiredSize));
                    handles[i] = handle;
                }
            }
            mutex.ReleaseMutex();
            return new(this, handles);
        }

        public SpatialCacheHandle Cache(ShadowAtlasHandle handle)
        {
            mutex.WaitOne();
            var cacheHandle = cache.AddToCache(handle.Handle);
            mutex.ReleaseMutex();
            handle.IsValid = false;
            return cacheHandle;
        }

        public SpatialCacheHandle[] CacheRange(ShadowAtlasRangeHandle handle)
        {
            SpatialCacheHandle[] cacheHandles = new SpatialCacheHandle[handle.Handles.Length];
            mutex.WaitOne();
            for (uint i = 0; i < cacheHandles.Length; i++)
            {
                cacheHandles[i] = cache.AddToCache(handle.Handles[i]);
            }
            mutex.ReleaseMutex();
            handle.IsValid = false;
            return cacheHandles;
        }

        public void Free(ShadowAtlasHandle handle)
        {
            mutex.WaitOne();
            handle.Handle.Dispose();
            mutex.ReleaseMutex();
            handle.IsValid = false;
        }

        public void FreeRange(ShadowAtlasRangeHandle handle)
        {
            var handles = handle.Handles;
            mutex.WaitOne();
            for (uint i = 0; i < handles.Length; i++)
            {
                handles[i].Dispose();
            }
            mutex.ReleaseMutex();
            handle.IsValid = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                OnDisposing?.Invoke(this);
                cache.Dispose();
                allocator.Dispose();
                texture.Dispose();
                disposedValue = true;
            }
        }

        ~ShadowAtlas()
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