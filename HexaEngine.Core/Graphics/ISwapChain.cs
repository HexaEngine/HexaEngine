namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Mathematics;

    /// <summary>
    /// Represents a swap chain for presenting graphics output to a display.
    /// </summary>
    public interface ISwapChain : IDisposable
    {
        /// <summary>
        /// Presents the next available frame to the screen.
        /// </summary>
        /// <param name="sync">A flag indicating whether to synchronize the presentation with the vertical sync (VSync) of the display.</param>
        void Present(bool sync);

        /// <summary>
        /// Presents the next available frame to the screen with depending on the settings synchronization.
        /// </summary>
        void Present();

        /// <summary>
        /// Waits for the completion of all pending rendering operations.
        /// </summary>
        void Wait();

        /// <summary>
        /// Resizes the swap chain to the specified width and height.
        /// </summary>
        /// <param name="width">The new width of the swap chain.</param>
        /// <param name="height">The new height of the swap chain.</param>
        void Resize(int width, int height);

        /// <summary>
        /// Waits for the presentation of the current frame to complete.
        /// </summary>
        void WaitForPresent();

        /// <summary>
        /// Occurs when the swap chain is being resized.
        /// </summary>
        event EventHandler? Resizing;

        /// <summary>
        /// Occurs when the swap chain has been resized.
        /// </summary>
        event EventHandler<ResizedEventArgs>? Resized;

        /// <summary>
        /// Occurs when the graphics device is removed.
        /// </summary>
        event EventHandler<DeviceRemovedEventArgs>? DeviceRemoved;

        /// <summary>
        /// Gets the back buffer texture of the swap chain.
        /// </summary>
        ITexture2D Backbuffer { get; }

        /// <summary>
        /// Gets the render target view (RTV) of the back buffer.
        /// </summary>
        IRenderTargetView BackbufferRTV { get; }

        /// <summary>
        /// Gets the depth stencil view (DSV) of the back buffer.
        /// </summary>
        IDepthStencilView BackbufferDSV { get; }

        /// <summary>
        /// Gets the width of the swap chain.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the swap chain.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the viewport associated with the swap chain.
        /// </summary>
        Viewport Viewport { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable vertical synchronization (VSync) for the swap chain.
        /// </summary>
        bool VSync { get; set; }

        /// <summary>
        /// Gets or sets the target frames per second (FPS) for the swap chain.
        /// </summary>
        int TargetFPS { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to limit the frames per second (FPS) to the target FPS.
        /// </summary>
        bool LimitFPS { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the swap chain is currently active and rendering.
        /// </summary>
        bool Active { get; set; }
    }
}