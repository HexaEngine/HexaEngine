namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe class D3D11Fence : DeviceChildBase, IFence
    {
        internal ComPtr<ID3D11Fence> fence;

        public D3D11Fence(ComPtr<ID3D11Fence> fence)
        {
            this.fence = fence;
            nativePointer = new(fence.Handle);
        }

        public ulong GetCompletedValue()
        {
            return fence.GetCompletedValue();
        }

        public void SetEventOnCompletion(ulong value, void* hEvent)
        {
            fence.SetEventOnCompletion(value, (nint)hEvent).ThrowIf();
        }

        protected override void DisposeCore()
        {
            fence.Release();
        }
    }
}