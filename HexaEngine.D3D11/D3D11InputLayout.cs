namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11InputLayout : DisposableBase, IInputLayout
    {
        private readonly ID3D11InputLayout* layout;

        internal D3D11InputLayout(ID3D11InputLayout* layout)
        {
            this.layout = layout;
            NativePointer = new(layout);
        }

        public IntPtr NativePointer { get; }
        public string? DebugName { get; set; } = string.Empty;

        public void Bind(IGraphicsContext context)
        {
            if (context is D3D11GraphicsContext graphicsContext)
            {
                graphicsContext.DeviceContext->IASetInputLayout(layout);
            }
        }

        protected override void DisposeCore()
        {
            layout->Release();
        }
    }
}