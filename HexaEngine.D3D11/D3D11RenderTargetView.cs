namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11RenderTargetView : DeviceChildBase, IRenderTargetView
    {
        internal readonly ComPtr<ID3D11RenderTargetView> rtv;

        public D3D11RenderTargetView(ComPtr<ID3D11RenderTargetView> rtv, RenderTargetViewDescription description)
        {
            this.rtv = rtv;
            nativePointer = new(rtv);
            Description = description;
        }

        public RenderTargetViewDescription Description { get; }

        protected override void DisposeCore()
        {
            rtv.Release();
        }
    }
}