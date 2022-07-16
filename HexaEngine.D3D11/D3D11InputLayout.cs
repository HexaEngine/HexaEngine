namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11InputLayout : DeviceChildBase, IInputLayout
    {
        private readonly ID3D11InputLayout* layout;

        internal D3D11InputLayout(ID3D11InputLayout* layout)
        {
            this.layout = layout;
            nativePointer = new(layout);
        }

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