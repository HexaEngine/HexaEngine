using System.Numerics;

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
        public IRenderTargetView[] RenderTargetViews;  // Array von Render Targets
        public IDepthStencilView DepthStencilView;     // Optionales Depth Stencil View
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

    public struct ClearValue
    {
        public ClearColorValue ColorValue;
        public ClearDepthStencilValue DepthStencilValue;
    }
}