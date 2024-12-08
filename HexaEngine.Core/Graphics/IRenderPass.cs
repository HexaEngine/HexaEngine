using System.Numerics;
using System.Runtime.InteropServices;

namespace HexaEngine.Core.Graphics
{
    public interface IRenderPass : IDeviceChild
    {
    }

    public interface ICommandBuffer : IGraphicsContext
    {
        void Begin();

        void End();
    }

    public struct RenderPassDesc
    {
        public IRenderTargetView[] RenderTargetViews;
        public IDepthStencilView? DepthStencilView;
        public ClearColorValue[]? RenderTargetClearValues;
        public ClearDepthStencilValue? DepthStencilClearValue;

        public bool IsMultisample;
        public int SampleCount;

        public RenderPassDesc(IRenderTargetView renderTargetView, IDepthStencilView? depthStencilView, ClearColorValue? color = null, ClearDepthStencilValue? depth = null)
        {
            RenderTargetViews = new IRenderTargetView[1];
            RenderTargetViews[0] = renderTargetView;
            DepthStencilView = depthStencilView;

            if (color.HasValue)
            {
                RenderTargetClearValues = [color.Value];
            }

            DepthStencilClearValue = depth;
        }

        public RenderPassDesc(IRenderTargetView[] renderTargetViews, IDepthStencilView? depthStencilView, ClearColorValue[]? colors = null, ClearDepthStencilValue? depth = null)
        {
            if (renderTargetViews.Length > BlendDescription.SimultaneousRenderTargetCount)
            {
                throw new ArgumentOutOfRangeException($"'renderTargetViews.Length' can only be <= {BlendDescription.SimultaneousRenderTargetCount}, but was {renderTargetViews.Length}");
            }

            if (colors?.Length > BlendDescription.SimultaneousRenderTargetCount)
            {
                throw new ArgumentOutOfRangeException($"'colors.Length' can only be <= {BlendDescription.SimultaneousRenderTargetCount}, but was {colors.Length}");
            }

            RenderTargetViews = renderTargetViews;
            DepthStencilView = depthStencilView;
            RenderTargetClearValues = colors;
            DepthStencilClearValue = depth;
        }
    }

    public struct ClearColorValue
    {
        public Vector4 Color;
    }

    public struct ClearDepthStencilValue
    {
        public float depth;
        public uint stencil;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ClearValue
    {
        [FieldOffset(0)]
        public ClearColorValue ColorValue;

        [FieldOffset(0)]
        public ClearDepthStencilValue DepthStencilValue;
    }
}