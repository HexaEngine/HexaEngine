namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11DepthStencilView : DisposableBase, IDepthStencilView
    {
        private readonly ID3D11DepthStencilView* dsv;

        public D3D11DepthStencilView(ID3D11DepthStencilView* dsv, DepthStencilViewDescription description)
        {
            this.dsv = dsv;
            NativePointer = new(dsv);
            Description = description;
        }

        public DepthStencilViewDescription Description { get; }

        public IntPtr NativePointer { get; }

        public string? DebugName { get; set; } = string.Empty;

        protected override void DisposeCore()
        {
            dsv->Release();
        }
    }
}