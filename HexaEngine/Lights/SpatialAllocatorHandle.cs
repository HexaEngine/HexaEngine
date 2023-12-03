namespace HexaEngine.Lights
{
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public unsafe class SpatialAllocatorHandle : IDisposable
    {
        private readonly SpatialAllocator allocator;
        private SpatialAllocator.Space* space;
        private bool disposedValue;

        internal SpatialAllocatorHandle(SpatialAllocator allocator, SpatialAllocator.Space* space)
        {
            this.allocator = allocator;
            this.space = space;
        }

        public SpatialAllocator Allocator => allocator;

        internal SpatialAllocator.Space* Space => space;

        public Vector2 Offset => space->Offset;

        public Vector2 Size => space->Size;

        public Viewport Viewport => new(Offset, Size);

        public bool IsValid => space != null;

        public void Reallocate(Vector2 size)
        {
            if (space != null)
            {
                allocator.Free(space);
            }

            space = allocator.AllocInternal(size);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                allocator.Free(space);
                space = null;
                disposedValue = true;
            }
        }

        ~SpatialAllocatorHandle()
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

        public override string ToString()
        {
            return space->ToString();
        }
    }
}