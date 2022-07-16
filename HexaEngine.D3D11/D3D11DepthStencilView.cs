namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11DepthStencilView : DeviceChildBase, IDepthStencilView
    {
        private readonly ID3D11DepthStencilView* dsv;

        public D3D11DepthStencilView(ID3D11DepthStencilView* dsv, DepthStencilViewDescription description)
        {
            this.dsv = dsv;
            nativePointer = new(dsv);
            Description = description;
        }

        public DepthStencilViewDescription Description { get; }

        protected override void DisposeCore()
        {
            dsv->Release();
        }
    }
}