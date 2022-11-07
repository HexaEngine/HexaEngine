namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11ClassInstance : DeviceChildBase, IClassInstance
    {
        internal readonly ID3D11ClassInstance* classInstance;

        public D3D11ClassInstance(ID3D11ClassInstance* classInstance)
        {
            this.classInstance = classInstance;
            nativePointer = new(classInstance);
        }

        protected override void DisposeCore()
        {
            classInstance->Release();
        }
    }
}