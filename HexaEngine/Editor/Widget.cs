namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;

    public abstract class Widget
    {
        public abstract void Init(IGraphicsDevice device);

        public abstract void Draw(IGraphicsContext context);

        public abstract void Dispose();
    }
}