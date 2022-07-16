namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11SamplerState : DeviceChildBase, ISamplerState
    {
        private readonly ID3D11SamplerState* sampler;

        public D3D11SamplerState(ID3D11SamplerState* sampler, SamplerDescription description)
        {
            this.sampler = sampler;
            nativePointer = new(sampler);
            Description = description;
        }

        public SamplerDescription Description { get; }

        protected override void DisposeCore()
        {
            sampler->Release();
        }
    }
}