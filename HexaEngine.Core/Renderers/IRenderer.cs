namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Scenes;

    public interface IRendererComponent : IComponent
    {
        public uint QueueIndex { get; }

        public void Update(IGraphicsContext context);

        public void VisibilityTest(IGraphicsContext context);

        public void DrawDepth(IGraphicsContext context, IBuffer camera);

        public void Draw(IGraphicsContext context);
    }
}