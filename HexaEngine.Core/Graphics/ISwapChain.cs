namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Events;
    using HexaEngine.Mathematics;

    public interface ISwapChain : IDisposable
    {
        void Present(uint syncInt);

        void Resize(int width, int height);

        event EventHandler? Resizing;

        event EventHandler<ResizedEventArgs>? Resized;

        ITexture2D Backbuffer { get; }

        IRenderTargetView BackbufferRTV { get; }

        IDepthStencilView BackbufferDSV { get; }
        int Width { get; }
        int Height { get; }
        Viewport Viewport { get; }
    }
}