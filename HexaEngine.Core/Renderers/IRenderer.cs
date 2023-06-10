namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;

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

    public interface IRendererComponent : IComponent, IHasFlags<RendererFlags>
    {
        public uint QueueIndex { get; }

        public BoundingBox BoundingBox { get; }

        public void Update(IGraphicsContext context);

        public void VisibilityTest(IGraphicsContext context);

        public void DrawDepth(IGraphicsContext context);

        public void DrawShadows(IGraphicsContext context, IBuffer light, ShadowType type);

        public void Draw(IGraphicsContext context);
    }
}