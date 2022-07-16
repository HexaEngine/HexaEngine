namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11DepthStencilState : DeviceChildBase, IDepthStencilState
    {
        private readonly ID3D11DepthStencilState* state;

        internal D3D11DepthStencilState(ID3D11DepthStencilState* state, DepthStencilDescription description)
        {
            this.state = state;
            nativePointer = new(state);
            Description = description;
        }

        public DepthStencilDescription Description { get; }

        protected override void DisposeCore()
        {
            state->Release();
        }
    }
}