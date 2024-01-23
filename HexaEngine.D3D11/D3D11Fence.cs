namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11Fence : DeviceChildBase, IFence
    {
        internal ComPtr<ID3D11Fence> fence;

        public D3D11Fence(ComPtr<ID3D11Fence> fence)
        {
            this.fence = fence;
            nativePointer = new(fence);
        }

        public ulong GetCompletedValue()
        {
            return fence.GetCompletedValue();
        }

        public void SetEventOnCompletion(ulong value, void* hEvent)
        {
            fence.SetEventOnCompletion(value, hEvent).ThrowHResult();
        }

        protected override void DisposeCore()
        {
            throw new NotImplementedException();
        }
    }
}