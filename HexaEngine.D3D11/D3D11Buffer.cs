namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;
    using ResourceDimension = Core.Graphics.ResourceDimension;

    public unsafe class D3D11Buffer : DisposableBase, IBuffer
    {
        private readonly ID3D11Buffer* buffer;

        internal D3D11Buffer(ID3D11Buffer* buffer, BufferDescription desc)
        {
            this.buffer = buffer;
            NativePointer = new(buffer);
            Description = desc;
            Length = desc.ByteWidth;
        }

        public BufferDescription Description { get; }

        public int Length { get; }

        public ResourceDimension Dimension => ResourceDimension.Buffer;

        public IntPtr NativePointer { get; }

        public string? DebugName { get; set; } = string.Empty;

        protected override void DisposeCore()
        {
            buffer->Release();
        }
    }
}