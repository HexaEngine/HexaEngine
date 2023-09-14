namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Mathematics;

    public interface ISwapChain : IDisposable
    {
        void Present(bool sync);

        void Present();

        void Wait();

        void Resize(int width, int height);

        void WaitForPresent();

        event EventHandler? Resizing;

        event EventHandler<ResizedEventArgs>? Resized;

        event EventHandler<DeviceRemovedEventArgs>? DeviceRemoved;

        ITexture2D Backbuffer { get; }

        IRenderTargetView BackbufferRTV { get; }

        IDepthStencilView BackbufferDSV { get; }
        int Width { get; }
        int Height { get; }
        Viewport Viewport { get; }
        bool VSync { get; set; }
        int TargetFPS { get; set; }
        bool LimitFPS { get; set; }

        bool Active { get; set; }
    }
}