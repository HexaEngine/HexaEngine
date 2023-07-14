namespace HexaEngine.Core.Graphics.Buffers
{
    public interface IStructuredBuffer
    {
        uint Capacity { get; set; }

        uint Count { get; }

        string? DebugName { get; set; }

        BufferDescription Description { get; }

        ResourceDimension Dimension { get; }

        bool IsDisposed { get; }

        int Length { get; }

        nint NativePointer { get; }

        IShaderResourceView SRV { get; }

        event EventHandler? OnDisposed;

        void Dispose();

        void EnsureCapacity(uint capacity);

        void RemoveAt(int index);

        void ResetCounter();

        bool Update(IGraphicsContext context);
    }

    public interface IStructuredBuffer<T> : IStructuredBuffer where T : unmanaged
    {
        T this[int index] { get; set; }

        void Add(T item);
    }
}