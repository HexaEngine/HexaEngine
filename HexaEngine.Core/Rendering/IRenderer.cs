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
        Draw = 4,
        DrawIndirect = 8,
        Culling = 16,
        CastShadows = 32,
        NoDepthTest = 64,
        All = Update | Depth | Draw | Culling | CastShadows,
    }

    public interface IRendererComponent : IComponent, IHasFlags<RendererFlags>
    {
        public uint QueueIndex { get; }

        public BoundingBox BoundingBox { get; }

        public void Update(IGraphicsContext context);

        public void VisibilityTest(IGraphicsContext context, Camera camera);

        public void DrawDepth(IGraphicsContext context);

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        public void Draw(IGraphicsContext context);
    }
}