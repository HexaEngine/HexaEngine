namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Mathematics;
    using Silk.NET.Core.Contexts;
    using Silk.NET.OpenGL;
    using System;

    public class OpenGLSwapChain : ISwapChain
    {
        private readonly GL gl;
        private readonly IGLContext context;
        private readonly IWindow window;
        private bool vsync;
        private int targetFPS;
        private bool limitFPS;
        private bool active;

        public OpenGLSwapChain(GL gl, IGLContext context, IWindow window)
        {
            this.gl = gl;
            this.context = context;
            this.window = window;
        }

        public ITexture2D Backbuffer => throw new NotImplementedException();

        public IRenderTargetView BackbufferRTV => throw new NotImplementedException();

        public IDepthStencilView BackbufferDSV => throw new NotImplementedException();

        public int Width => window.Width;

        public int Height => window.Height;

        public Viewport Viewport => new(window.Width, window.Height);

        public bool VSync { get => vsync; set => vsync = value; }

        public int TargetFPS { get => targetFPS; set => targetFPS = value; }

        public bool LimitFPS { get => limitFPS; set => limitFPS = value; }

        public bool Active { get => active; set => active = value; }

        public event EventHandler? Resizing;

        public event EventHandler<ResizedEventArgs>? Resized;

        public void Dispose()
        {
        }

        public void Present(bool sync)
        {
            context.SwapInterval(sync ? 1 : 0);
        }

        public void Present()
        {
            context.SwapBuffers();
        }

        public void Resize(int width, int height)
        {
        }

        public void Wait()
        {
        }
    }
}