namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11CommandList : DeviceChildBase, ICommandList
    {
        internal readonly ID3D11CommandList* commandList;

        public D3D11CommandList(ID3D11CommandList* commandList)
        {
            this.commandList = commandList;
            nativePointer = new(commandList);
        }

        protected override void DisposeCore()
        {
            commandList->Release();
        }
    }
}