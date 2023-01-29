namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;

    public interface IImGuiWindow
    {
        void Dispose();
        void DrawContent(IGraphicsContext context);
        void DrawMenu();
        void DrawWindow(IGraphicsContext context);
        void Init(IGraphicsDevice device);
    }
}