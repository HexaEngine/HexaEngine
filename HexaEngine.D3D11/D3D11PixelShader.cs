namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11PixelShader : DeviceChildBase, IPixelShader
    {
        internal readonly ID3D11PixelShader* ps;

        internal D3D11PixelShader(ID3D11PixelShader* ps)
        {
            this.ps = ps;
            nativePointer = new(ps);
        }

        public void Bind(IGraphicsContext context)
        {
            if (context is D3D11GraphicsContext graphicsContext)
            {
                graphicsContext.DeviceContext->PSSetShader(ps, null, 0);
            }
        }

        protected override void DisposeCore()
        {
            ps->Release();
        }
    }
}