namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe class D3D11Query : DeviceChildBase, IQuery
    {
        internal readonly ComPtr<ID3D11Query> query;

        public D3D11Query(ComPtr<ID3D11Query> query)
        {
            this.query = query;
            nativePointer = new(query.Handle);
        }

        protected override void DisposeCore()
        {
            query.Release();
        }
    }
}