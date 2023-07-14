namespace HexaEngine.Core.Graphics.Buffers
{
    public interface IIndexBuffer
    {
        uint Capacity { get; set; }

        uint Count { get; }

        string? DebugName { get; set; }

        BufferDescription Description { get; }

        ResourceDimension Dimension { get; }

        bool IsDisposed { get; }

        int Length { get; }

        nint NativePointer { get; }

        event EventHandler? OnDisposed;

        void Bind(IGraphicsContext context);

        void Bind(IGraphicsContext context, int offset);

        void Clear();

        void CopyTo(IGraphicsContext context, IBuffer buffer);

        void Dispose();

        void EnsureCapacity(uint capacity);

        void FlushMemory();

        void RemoveAt(int index);

        void ResetCounter();

        void Unbind(IGraphicsContext context);

        bool Update(IGraphicsContext context);
    }

    public interface IIndexBuffer<T> : IIndexBuffer where T : unmanaged
    {
        T this[int index] { get; set; }

        void Add(params T[] indices);

        void Add(T value);
    }
}