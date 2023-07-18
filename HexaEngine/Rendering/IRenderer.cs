namespace HexaEngine.Rendering
{
    using HexaEngine.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;

    public interface IRendererComponent : IComponent, IHasFlags<RendererFlags>
    {
        public uint QueueIndex { get; }

        public BoundingBox BoundingBox { get; }

        public void Update(IGraphicsContext context);

        public void VisibilityTest(CullingContext context);

        public void DrawDepth(IGraphicsContext context);

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        public void Draw(IGraphicsContext context, RenderPath path);

        public void Bake(IGraphicsContext context);
    }
}