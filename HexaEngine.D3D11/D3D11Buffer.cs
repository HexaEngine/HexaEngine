namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using ResourceDimension = Core.Graphics.ResourceDimension;

    public unsafe class D3D11Buffer : DeviceChildBase, IBuffer
    {
        internal readonly ComPtr<ID3D11Buffer> buffer;

        internal D3D11Buffer(ComPtr<ID3D11Buffer> buffer, BufferDescription desc)
        {
            this.buffer = buffer;
            nativePointer = new(buffer.Handle);
            Description = desc;
            Length = desc.ByteWidth;
        }

        public BufferDescription Description { get; }

        public int Length { get; }

        public ResourceDimension Dimension => ResourceDimension.Buffer;

        protected override void DisposeCore()
        {
            buffer.Release();
        }
    }
}