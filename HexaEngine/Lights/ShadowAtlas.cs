namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    public unsafe class ShadowAtlas : IDisposable
    {
        private readonly string dbgName;
        private readonly int size;
        private readonly int layerCount;

        private readonly Mutex mutex = new();
        private readonly DepthStencil texture;
        private readonly SpatialAllocator allocator;

        private bool disposedValue;

        public ShadowAtlas(IGraphicsDevice device, ShadowAtlasDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"ShadowAtlas: {Path.GetFileName(filename)}, Line: {lineNumber}";
            size = description.Size;
            layerCount = description.Layers;
            allocator = new(new(size), layerCount);
            texture = new(device, description.Format, description.Size, description.Size, filename: filename, lineNumber: lineNumber);
            texture.DebugName = dbgName;
        }

        public ShadowAtlas(IGraphicsDevice device, int size = 8192, int layerCount = 8, Format format = Format.D32Float, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"ShadowAtlas: {Path.GetFileName(filename)}, Line: {lineNumber}";
            this.size = size;
            this.layerCount = layerCount;
            allocator = new(new(size), layerCount);
            texture = new(device, format, size, size);
        }

        public IDepthStencilView DSV => texture.DSV;

        public IShaderResourceView SRV => texture.SRV;

        public string DebugName => dbgName;

        public Viewport Viewport => texture.Viewport;

        public int LayerCount => layerCount;

        public float Size => size;

        public void Clear()
        {
            mutex.WaitOne();
            allocator.Clear();
            mutex.ReleaseMutex();
        }

        public ShadowAtlasHandle Alloc(int desiredSize)
        {
            return desiredSize <= 0 ? throw new ArgumentOutOfRangeException(nameof(desiredSize)) : Alloc((uint)desiredSize);
        }

        public ShadowAtlasHandle Alloc(uint desiredSize)
        {
            mutex.WaitOne();
            var handle = allocator.Alloc(new(desiredSize));
            mutex.ReleaseMutex();

            return new(this, handle);
        }

        public ShadowAtlasRangeHandle AllocRange(int desiredSize, int count)
        {
            return desiredSize <= 0 ? throw new ArgumentOutOfRangeException(nameof(desiredSize)) : AllocRange((uint)desiredSize, count);
        }

        public ShadowAtlasRangeHandle AllocRange(uint desiredSize, int count)
        {
            SpatialAllocatorHandle[] allocations = new SpatialAllocatorHandle[count];
            for (uint i = 0; i < count; i++)
            {
                allocations[i] = Alloc(desiredSize).Handle;
            }
            return new(this, allocations);
        }

        public void Free(ref ShadowAtlasHandle handle)
        {
            mutex.WaitOne();
            handle.Handle.Dispose();
            mutex.ReleaseMutex();
            handle.IsValid = false;
        }

        public void FreeRange(ref ShadowAtlasRangeHandle handle)
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