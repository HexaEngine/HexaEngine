namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11DepthStencilState : DisposableBase, IDepthStencilState
    {
        private readonly ID3D11DepthStencilState* state;

        internal D3D11DepthStencilState(ID3D11DepthStencilState* state, DepthStencilDescription description)
        {
            this.state = state;
            NativePointer = new(state);
            Description = description;
        }

        public IntPtr NativePointer { get; }

        public string? DebugName { get; set; } = string.Empty;

        public DepthStencilDescription Description { get; }

        protected override void DisposeCore()
        {
            state->Release();
        }
    }
}