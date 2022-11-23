namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;

    public interface IEffect : IDisposable
    {
        void Draw(IGraphicsContext context);
    }
}