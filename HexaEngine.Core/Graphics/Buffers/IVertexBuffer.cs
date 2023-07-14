namespace HexaEngine.Core.Graphics.Buffers
{
    public interface IVertexBuffer
    {
        uint Capacity { get; set; }

        uint Count { get; }

        string? DebugName { get; set; }

        BufferDescription Description { get; }

        ResourceDimension Dimension { get; }

        bool IsDisposed { get; }

        int Length { get; }

        nint NativePointer { get; }

        uint Stride { get; }

        event EventHandler? OnDisposed;

        void Bind(IGraphicsContext context);

        void Clear();

        void CopyTo(IGraphicsContext context, IBuffer buffer);

        void Dispose();

        void EnsureCapacity(uint capacity);

        void FlushMemory();

        void Remove(int index);

        void ResetCounter();

        void Unbind(IGraphicsContext context);

        bool Update(IGraphicsContext context);
    }

    public interface IVertexBuffer<T> : IVertexBuffer where T : unmanaged
    {
        T this[int index] { get; set; }

        void Add(params T[] vertices);
    }
}