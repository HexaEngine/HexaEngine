namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11RasterizerState : DeviceChildBase, IRasterizerState
    {
        internal readonly ID3D11RasterizerState* state;

        internal D3D11RasterizerState(ID3D11RasterizerState* state, RasterizerDescription description)
        {
            this.state = state;
            nativePointer = new(state);
            Description = description;
        }

        public RasterizerDescription Description { get; }

        protected override void DisposeCore()
        {
            state->Release();
        }
    }
}