namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11ClassLinkage : DeviceChildBase, IClassLinkage
    {
        internal readonly ID3D11ClassLinkage* classLinkage;

        public D3D11ClassLinkage(ID3D11ClassLinkage* classLinkage)
        {
            this.classLinkage = classLinkage;
            nativePointer = new(classLinkage);
        }

        public IClassInstance CreateClassInstance(string name, uint cbOffset, uint cvOffset, uint texOffset, uint samplerOffset)
        {
            ID3D11ClassInstance* instance;
            classLinkage->CreateClassInstance(Utils.ToBytes(name), cbOffset, cvOffset, texOffset, samplerOffset, &instance);
            return new D3D11ClassInstance(instance);
        }

        public IClassInstance GetClassInstance(string name, uint index)
        {
            ID3D11ClassInstance* instance;
            classLinkage->GetClassInstance(Utils.ToBytes(name), index, &instance);
            return new D3D11ClassInstance(instance);
        }

        protected override void DisposeCore()
        {
            classLinkage->Release();
        }
    }
}