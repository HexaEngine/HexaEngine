namespace HexaEngine.Core.Windows
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Threading;
    using Hexa.NET.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents a core window.
    /// </summary>
    public interface ICoreWindow : IWindow, IDisposable
    {
        /// <summary>
        /// Gets the render dispatcher associated with the render window.
        /// </summary>
        IThreadDispatcher Dispatcher { get; }

        /// <summary>
        /// Gets the audio device associated with the core window.
        /// </summary>
        IAudioDevice AudioDevice { get; }

        /// <summary>
        /// Gets the graphics device associated with the core window.
        /// </summary>
        IGraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// Gets the graphics context associated with the core window.
        /// </summary>
        IGraphicsContext GraphicsContext { get; }

        /// <summary>
        /// Gets the swap chain associated with the core window.
        /// </summary>
        ISwapChain SwapChain { get; }

        /// <summary>
        /// Gets the viewport for rendering operations.
        /// </summary>
        Viewport RenderViewport { get; }

        /// <summary>
        /// Gets the viewport for the window.
        /// </summary>
        Viewport WindowViewport { get; }

        /// <summary>
        /// Initializes the render window with the specified audio and graphics devices.
        /// </summary>
        /// <param name="audioDevice">The audio device to use.</param>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice);

        /// <summary>
        /// Renders the graphics using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for rendering.</param>
        void Render(IGraphicsContext context);
    }
}