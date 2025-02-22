namespace HexaEngine.Core.Windows
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.SDL2;
    using Hexa.NET.Utilities.Threading;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using System;

    public abstract class CoreWindow : SdlWindow, ICoreWindow
    {
        private bool disposedValue;

        /// <summary>
        /// Creates a new instance of the <see cref="CoreWindow"/> class.
        /// </summary>
        public CoreWindow(SDLWindowFlags flags = SDLWindowFlags.Resizable) : base(flags)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CoreWindow"/> class.
        /// </summary>
        public CoreWindow(int x, int y, int width, int height, SDLWindowFlags flags = SDLWindowFlags.Resizable) : base(x, y, width, height, flags)
        {
        }

        public IThreadDispatcher Dispatcher { get; private set; } = null!;

        public IAudioDevice AudioDevice { get; private set; } = null!;

        public IGraphicsDevice GraphicsDevice { get; private set; } = null!;

        public IGraphicsContext GraphicsContext { get; private set; } = null!;

        public ISwapChain SwapChain { get; private set; } = null!;

        public abstract Viewport RenderViewport { get; }

        public abstract Viewport WindowViewport { get; }

        public virtual void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice)
        {
            AudioDevice = audioDevice;
            GraphicsDevice = graphicsDevice;
            GraphicsContext = graphicsDevice.Context;
            SwapChain = graphicsDevice.CreateSwapChain(this) ?? throw new PlatformNotSupportedException();
            SwapChain.Active = true;
            Dispatcher = new ThreadDispatcher(Thread.CurrentThread);
        }

        public abstract void Render(IGraphicsContext context);

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                SwapChain.Dispose();
                DisposeCore();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}