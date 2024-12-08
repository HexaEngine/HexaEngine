namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe class D3D11ShaderResourceView : DeviceChildBase, IShaderResourceView
    {
        internal readonly ComPtr<ID3D11ShaderResourceView> srv;

        public D3D11ShaderResourceView(ComPtr<ID3D11ShaderResourceView> srv, ShaderResourceViewDescription description)
        {
            this.srv = srv;
            nativePointer = new(srv.Handle);
            Description = description;
        }

        public ShaderResourceViewDescription Description { get; }

        protected override void DisposeCore()
        {
            srv.Release();
        }
    }
}