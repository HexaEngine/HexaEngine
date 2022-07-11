namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class D3D11BlendState : DisposableBase, IBlendState
    {
        private readonly ID3D11BlendState* blend;

        internal D3D11BlendState(ID3D11BlendState* blend, BlendDescription description)
        {
            this.blend = blend;
            NativePointer = new(blend);
            Description = description;
        }

        public IntPtr NativePointer { get; }

        public string? DebugName { get; set; } = string.Empty;

        public BlendDescription Description { get; }

        protected override void DisposeCore()
        {
            blend->Release();
        }
    }
}