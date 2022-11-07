namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11DomainShader : DeviceChildBase, IDomainShader
    {
        internal readonly ID3D11DomainShader* ds;

        internal D3D11DomainShader(ID3D11DomainShader* ds)
        {
            this.ds = ds;
            nativePointer = new(ds);
        }

        public void Bind(IGraphicsContext context)
        {
            if (context is D3D11GraphicsContext graphicsContext)
            {
                graphicsContext.DeviceContext->DSSetShader(ds, null, 0);
            }
        }

        protected override void DisposeCore()
        {
            ds->Release();
        }
    }
}