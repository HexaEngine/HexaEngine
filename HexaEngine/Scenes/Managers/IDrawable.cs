namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;

    public interface IDrawable
    {
        void Update(IGraphicsContext context);

        void Draw(IGraphicsContext context, RenderQueueIndex index);
    }
}