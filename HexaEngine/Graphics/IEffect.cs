namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;

    public interface IEffect : IDisposable
    {
        void Reload();

        void Draw(IGraphicsContext context);

        void DrawSettings();
    }
}