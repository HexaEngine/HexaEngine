namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Scenes;

    [Flags]
    public enum RendererFlags
    {
        None = 0,
        Update = 1,
        Depth = 2,
        Geometry = 4,
        Culling = 8,
        CastShadows = 16,
        All = Update | Depth | Geometry | Culling | CastShadows,
    }

    public interface IRendererComponent : IComponent
    {
        public uint QueueIndex { get; }

        public RendererFlags Flags { get; }

        public void Update(IGraphicsContext context);

        public void VisibilityTest(IGraphicsContext context);

        public void DrawDepth(IGraphicsContext context, IBuffer camera);

        public void DrawShadows(IGraphicsContext context, IBuffer light, ShadowType type);

        public void Draw(IGraphicsContext context);
    }
}