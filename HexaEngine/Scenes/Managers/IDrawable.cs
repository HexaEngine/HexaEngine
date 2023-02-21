namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Rendering;

    public interface IDrawable
    {
        void Update(IGraphicsContext context);

        void Draw(IGraphicsContext context, RenderQueueIndex index);

        void DrawDepth(IGraphicsContext context);
    }
}