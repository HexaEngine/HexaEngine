namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class OpenGLProfiler : IGPUProfiler
    {
        public double this[string index] => 0;

        public bool Enabled { get; set; }
        public bool DisableLogging { get; set; }
        public IReadOnlyList<string> BlockNames { get; } = [];

        public void Begin(IGraphicsContext context, string name)
        {
        }

        public void Begin(string name)
        {
            throw new NotImplementedException();
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

        public void End(string name)
        {
            throw new NotImplementedException();
        }

        public void EndFrame(IGraphicsContext context)
        {
        }

        public void EndFrame()
        {
            throw new NotImplementedException();
        }
    }
}