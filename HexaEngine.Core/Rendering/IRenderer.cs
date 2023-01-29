namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;

    public interface IRenderer : IDisposable
    {
        void Init(IGraphicsDevice device);

        void Update(IGraphicsContext context);

        void Draw(IGraphicsContext context);
    }
}