namespace HexaEngine.D3D12
{
    public unsafe class D3D12CommandQueue
    {
        internal ComPtr<ID3D12CommandQueue> CommandQueue;

        public D3D12CommandQueue(ComPtr<ID3D12CommandQueue> commandQueue)
        {
            CommandQueue = commandQueue;
        }
    }
}