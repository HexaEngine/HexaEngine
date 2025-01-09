using Hexa.NET.Mathematics;

namespace HexaEngine.Core.Graphics
{
    public struct BeginDrawDesc
    {
        public RenderTargetArray RenderTargets;
        public IDepthStencilView? DepthStencil;
        public uint NumViews;
        public ViewportArray Viewports;
        public ClearColors ClearColors;
        public ClearDepthStencilValue ClearDepthStencilValue;
        public ClearFlags ClearFlags;

        public BeginDrawDesc(IRenderTargetView rtv, IDepthStencilView? dsv, Viewport viewport, ClearColorValue clearColor = default, ClearDepthStencilValue clearDepthStencilValue = default, ClearFlags flags = ClearFlags.None)
        {
            RenderTargets[0] = rtv;
            DepthStencil = dsv;
            Viewports[0] = viewport;
            ClearColors[0] = clearColor;
            ClearDepthStencilValue = clearDepthStencilValue;
            ClearFlags = flags;
            NumViews = 1;
        }

        public BeginDrawDesc(RenderTargetArray renderTargets, IDepthStencilView? depthStencil, uint numViews, ViewportArray viewports, ClearColors clearColors = default, ClearDepthStencilValue clearDepthStencilValue = default, ClearFlags clearFlags = ClearFlags.None)
        {
            RenderTargets = renderTargets;
            DepthStencil = depthStencil;
            NumViews = numViews;
            Viewports = viewports;
            ClearColors = clearColors;
            ClearDepthStencilValue = clearDepthStencilValue;
            ClearFlags = clearFlags;
        }

        public BeginDrawDesc(Texture2D target, IDepthStencilView? dsv, ClearColorValue clearColor = default, ClearDepthStencilValue clearDepthStencilValue = default, ClearFlags flags = ClearFlags.None)
        {
            RenderTargets[0] = target;
            DepthStencil = dsv;
            Viewports[0] = target.Viewport;
            ClearColors[0] = clearColor;
            ClearDepthStencilValue = clearDepthStencilValue;
            ClearFlags = flags;
            NumViews = 1;
        }
    }
}