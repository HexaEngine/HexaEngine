using Hexa.NET.SDL3;

namespace CrashReporter
{
    internal unsafe class BindingsContext : HexaGen.Runtime.IGLContext
    {
        private readonly SDLWindow* window;
        private readonly SDLGLContext context;

        public BindingsContext(SDLWindow* window, SDLGLContext context)
        {
            this.window = window;
            this.context = context;
        }

        public nint Handle => (nint)window;

        public bool IsCurrent => SDL.GLGetCurrentContext() == context;

        public void Dispose()
        {
        }

        public nint GetProcAddress(string procName)
        {
            return (nint)SDL.GLGetProcAddress(procName);
        }

        public bool IsExtensionSupported(string extensionName)
        {
            return SDL.GLExtensionSupported(extensionName);
        }

        public void MakeCurrent()
        {
            SDL.GLMakeCurrent(window, context);
        }

        public void SwapBuffers()
        {
            SDL.GLSwapWindow(window);
        }

        public void SwapInterval(int interval)
        {
            SDL.GLSetSwapInterval(interval);
        }

        public bool TryGetProcAddress(string procName, out nint procAddress)
        {
            procAddress = (nint)SDL.GLGetProcAddress(procName);
            return procAddress != 0;
        }
    }
}