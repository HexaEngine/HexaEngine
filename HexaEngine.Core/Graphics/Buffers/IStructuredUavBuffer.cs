namespace HexaEngine.Core.Graphics.Buffers
{
    public interface IStructuredUavBuffer
    {
        bool CanRead { get; }
        bool CanWrite { get; }
        uint Capacity { get; set; }
        IBuffer? CopyBuffer { get; }
        uint Count { get; }
        string? DebugName { get; set; }
        BufferDescription Description { get; }
        ResourceDimension Dimension { get; }
        bool IsDisposed { get; }
        int Length { get; }
        nint NativePointer { get; }
        IShaderResourceView SRV { get; }
        IUnorderedAccessView UAV { get; }

        event EventHandler? OnDisposed;

        void Clear();

        void Clear(IGraphicsContext context);

        void CopyTo(IGraphicsContext context, IBuffer buffer);

        void Dispose();

        void EnsureCapacity(uint capacity);

        void Increment();

        void Read(IGraphicsContext context);

        void RemoveAt(int index);

        void RemoveAt(uint index);

        void ResetCounter();

        void SetDirty();

        bool Update(IGraphicsContext context);
    }

    public interface IStructuredUavBuffer<T> : IStructuredUavBuffer where T : unmanaged
    {
        T this[int index] { get; set; }

        T this[uint index] { get; set; }

        ref T Add(T args);
    }
}