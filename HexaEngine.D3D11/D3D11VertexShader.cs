namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11VertexShader : DeviceChildBase, IVertexShader
    {
        private readonly ID3D11VertexShader* vs;

        internal D3D11VertexShader(ID3D11VertexShader* vs)
        {
            this.vs = vs;
            nativePointer = new(vs);
        }

        public void Bind(IGraphicsContext context)
        {
            if (context is D3D11GraphicsContext graphicsContext)
            {
                graphicsContext.DeviceContext->VSSetShader(vs, null, 0);
            }
        }

        protected override void DisposeCore()
        {
            vs->Release();
        }
    }
}