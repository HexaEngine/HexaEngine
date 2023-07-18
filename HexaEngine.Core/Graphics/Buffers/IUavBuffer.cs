namespace HexaEngine.Core.Graphics.Buffers
{
    public interface IUavBuffer
    {
        IBuffer Buffer { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        IBuffer? CopyBuffer { get; }

        string? DebugName { get; set; }

        BufferDescription Description { get; }

        ResourceDimension Dimension { get; }

        bool IsDisposed { get; }

        uint Length { get; set; }

        nint NativePointer { get; }

        IShaderResourceView SRV { get; }

        IUnorderedAccessView UAV { get; }

        event EventHandler? OnDisposed;

        void Clear(IGraphicsContext context);

        void CopyTo(IGraphicsContext context, IBuffer buffer);

        void Dispose();

        unsafe void Read(IGraphicsContext context, void* dst, int length);

        unsafe void Write(IGraphicsContext context, void* src, int length);
    }
}