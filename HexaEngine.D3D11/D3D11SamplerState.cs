namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;

    public unsafe class D3D11SamplerState : DeviceChildBase, ISamplerState
    {
        internal readonly ComPtr<ID3D11SamplerState> sampler;

        public D3D11SamplerState(ComPtr<ID3D11SamplerState> sampler, SamplerStateDescription description)
        {
            this.sampler = sampler;
            nativePointer = new(sampler.Handle);
            Description = description;
        }

        public SamplerStateDescription Description { get; }

        protected override void DisposeCore()
        {
            sampler.Release();
        }
    }
}