namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe class D3D11ClassInstance : DeviceChildBase, IClassInstance
    {
        internal readonly ComPtr<ID3D11ClassInstance> classInstance;

        public D3D11ClassInstance(ComPtr<ID3D11ClassInstance> classInstance)
        {
            this.classInstance = classInstance;
            nativePointer = new(classInstance.Handle);
        }

        protected override void DisposeCore()
        {
            classInstance.Release();
        }
    }
}