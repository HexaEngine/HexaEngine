namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11CommandBuffer : D3D11GraphicsContextBase, ICommandBuffer
    {
        internal ComPtr<ID3D11CommandList> commandList;

        public D3D11CommandBuffer(IGraphicsDevice device, ComPtr<ID3D11DeviceContext4> context) : base(device, context)
        {
        }

        public D3D11CommandBuffer(IGraphicsDevice device, ComPtr<ID3D11DeviceContext3> context) : base(device, context)
        {
        }

        public void Begin()
        {
            ClearState();
            DisposeCore(); // doesnt set the disposed flag or raise the event.
        }

        public void End()
        {
            DeviceContext.FinishCommandList(false, ref commandList);
        }

        public void Signal(IFence fence, ulong value)
        {
            throw new NotSupportedException();
        }

        public void Wait(IFence fence, ulong value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeCore();
        }

        protected virtual void DisposeCore()
        {
            if (commandList.Handle != null)
            {
                commandList.Release();
                commandList = null;
            }
        }
    }
}