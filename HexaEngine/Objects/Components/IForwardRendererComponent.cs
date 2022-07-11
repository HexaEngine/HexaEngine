namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;

    public interface IEarlyForwardRendererComponent : IComponent
    {
        void Render(IGraphicsContext context, Viewport viewport, IView view);
    }

    public interface IDeferredRendererComponent : IComponent
    {
        void Render(IGraphicsContext context, Viewport viewport, IView view);
    }

    public interface IDepthRendererComponent : IComponent
    {
        void RenderDepthFrontface(IGraphicsContext context, Viewport viewport, IView view);

        void RenderDepthBackface(IGraphicsContext context, Viewport viewport, IView view);
    }

    public interface IForwardRendererComponent : IComponent
    {
        void Render(IGraphicsContext context, Viewport viewport, IView view);
    }
}