namespace HexaEngine.Core.Graphics
{
    public interface IGPUProfiler : IDisposable
    {
        double this[string index] { get; }

        void CreateBlock(string name);

        void DestroyBlock(string name);

        void BeginFrame();

        void EndFrame(IGraphicsContext context);

        void Begin(IGraphicsContext context, string name);

        void End(IGraphicsContext context, string name);
    }
}