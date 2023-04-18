namespace ImagePainter
{
    using HexaEngine.Core.Graphics;
    using System.Numerics;

    public abstract class Tool : IDisposable
    {
        public abstract string Icon { get; }

        public abstract string Name { get; }

        public abstract void Init(IGraphicsDevice device);

        public abstract void DrawSettings();

        public abstract void DrawPreview(Vector2 position, IGraphicsContext context);

        public abstract void Draw(Vector2 position, IGraphicsContext context);

        public abstract void Dispose();
    }
}