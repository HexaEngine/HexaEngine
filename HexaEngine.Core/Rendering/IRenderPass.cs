namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;

    public interface IRenderPass : IDisposable
    {
        void BeginDraw(IGraphicsContext context);

        void Draw(IGraphicsContext context);

        void EndDraw(IGraphicsContext context);
    }
}