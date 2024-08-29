namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11GraphicsContext : D3D11GraphicsContextBase, IGraphicsContext
    {
        protected ComPtr<ID3D11DeviceContext4> DeviceContext4;

        internal D3D11GraphicsContext(D3D11GraphicsDevice device) : base(device, device.DeviceContext)
        {
            DeviceContext4 = device.DeviceContext;
        }

        internal D3D11GraphicsContext(D3D11GraphicsDevice device, ComPtr<ID3D11DeviceContext3> context) : base(device, context)
        {
        }

        public void Signal(IFence fence, ulong value)
        {
            DeviceContext4.Signal(((D3D11Fence)fence).fence, value);
        }

        public void Wait(IFence fence, ulong value)
        {
            DeviceContext4.Wait(((D3D11Fence)fence).fence, value);
        }
    }
}