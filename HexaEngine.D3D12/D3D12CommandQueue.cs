namespace HexaEngine.D3D12
{
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D12;

    public unsafe class D3D12CommandQueue
    {
        internal ComPtr<ID3D12CommandQueue> CommandQueue;

        public D3D12CommandQueue(ComPtr<ID3D12CommandQueue> commandQueue)
        {
            CommandQueue = commandQueue;
        }
    }
}