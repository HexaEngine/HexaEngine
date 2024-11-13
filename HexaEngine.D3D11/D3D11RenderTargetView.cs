namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe class D3D11RenderTargetView : DeviceChildBase, IRenderTargetView
    {
        internal readonly ComPtr<ID3D11RenderTargetView> rtv;

        public D3D11RenderTargetView(ComPtr<ID3D11RenderTargetView> rtv, RenderTargetViewDescription description)
        {
            this.rtv = rtv;
            nativePointer = new(rtv.Handle);
            Description = description;
        }

        public RenderTargetViewDescription Description { get; }

        protected override void DisposeCore()
        {
            rtv.Release();
        }
    }
}