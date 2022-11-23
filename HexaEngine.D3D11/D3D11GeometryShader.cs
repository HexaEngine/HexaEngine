namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11GeometryShader : DeviceChildBase, IGeometryShader
    {
        internal readonly ID3D11GeometryShader* gs;

        internal D3D11GeometryShader(ID3D11GeometryShader* gs)
        {
            this.gs = gs;
            nativePointer = new(gs);
        }

        public void Bind(IGraphicsContext context)
        {
            if (context is D3D11GraphicsContext graphicsContext)
            {
                graphicsContext.DeviceContext->GSSetShader(gs, null, 0);
            }
        }

        protected override void DisposeCore()
        {
            gs->Release();
        }
    }
}