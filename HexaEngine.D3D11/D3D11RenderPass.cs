namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.Maths;
    using Rectangle = Hexa.NET.Mathematics.Rectangle;

    public unsafe class D3D11RenderPass : DeviceChildBase, IRenderPass
    {
        private readonly RenderPassDesc desc;

        private ID3D11RenderTargetView** rtvs;
        private uint rtvCount;
        private ID3D11DepthStencilView* dsv;

        public D3D11RenderPass(RenderPassDesc desc)
        {
            this.desc = desc;

            rtvs = (ID3D11RenderTargetView**)AllocT<nint>(desc.RenderTargetViews.Length);
            rtvCount = (uint)desc.RenderTargetViews.Length;
            for (int i = 0; i < desc.RenderTargetViews.Length; i++)
            {
                rtvs[i] = desc.RenderTargetViews[i].GetAs<ID3D11RenderTargetView>();
            }
            dsv = desc.DepthStencilView.GetAs<ID3D11DepthStencilView>();
        }

        public void Begin(ICommandBuffer context, Rectangle workingArea, uint clearValueCount, ClearValue* values)
        {
            ComPtr<ID3D11DeviceContext3> ctx = ((D3D11GraphicsContextBase)context).DeviceContext;

            Box2D<int> box = new(workingArea.Left, workingArea.Top, workingArea.Right, workingArea.Bottom);
            for (uint i = 0; i < rtvCount && i < clearValueCount; i++)
            {
                var rtv = rtvs[i];
                if (rtv == null) continue;
                ctx.ClearView((ID3D11View*)rtv, (float*)&values[i].ColorValue, &box, 1);
            }

            if (clearValueCount > 0 && dsv != null)
            {
                ctx.ClearView((ID3D11View*)dsv, (float*)&values[0].DepthStencilValue, &box, 1);
            }

            ctx.OMSetRenderTargets(rtvCount, rtvs, dsv);
        }

        public void End(ICommandBuffer context)
        {
            ComPtr<ID3D11DeviceContext3> ctx = ((D3D11GraphicsContextBase)context).DeviceContext;
            var rtvs = stackalloc nint[(int)rtvCount]; // we will never reach the stack limit cuz the rtv limit is 8. See D3D11.SimultaneousRenderTargetCount.
            ctx.OMSetRenderTargets(rtvCount, (ID3D11RenderTargetView**)rtvs, (ID3D11DepthStencilView*)null);
        }

        protected override void DisposeCore()
        {
            if (rtvs != null)
            {
                Free(rtvs);
                rtvs = null;
            }
        }
    }
}