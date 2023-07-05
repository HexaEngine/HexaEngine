namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;

    public interface IRendererComponent : IComponent, IHasFlags<RendererFlags>
    {
        public uint QueueIndex { get; }

        public BoundingBox BoundingBox { get; }

        public void Update(IGraphicsContext context);

        public void VisibilityTest(IGraphicsContext context, Camera camera);

        public void DrawDepth(IGraphicsContext context);

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        public void Draw(IGraphicsContext context, RenderPath path);

        public void Bake(IGraphicsContext context);
    }
}