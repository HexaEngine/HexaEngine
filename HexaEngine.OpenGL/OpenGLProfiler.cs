namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;

    public class OpenGLProfiler : IGPUProfiler
    {
        public double this[string index] => 0;

        public bool Enabled { get; set; }
        public bool DisableLogging { get; set; }

        public void Begin(IGraphicsContext context, string name)
        {
        }

        public void BeginFrame()
        {
        }

        public void CreateBlock(string name)
        {
        }

        public void DestroyBlock(string name)
        {
        }

        public void Dispose()
        {
        }

        public void End(IGraphicsContext context, string name)
        {
        }

        public void EndFrame(IGraphicsContext context)
        {
        }
    }
}