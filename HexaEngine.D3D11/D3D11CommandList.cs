namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    [Obsolete("Use command buffers")]
    public unsafe class D3D11CommandList : DeviceChildBase, ICommandList
    {
        internal readonly ComPtr<ID3D11CommandList> commandList;

        public D3D11CommandList(ComPtr<ID3D11CommandList> commandList)
        {
            this.commandList = commandList;
            nativePointer = new(commandList.Handle);
        }

        protected override void DisposeCore()
        {
            commandList.Release();
        }
    }
}