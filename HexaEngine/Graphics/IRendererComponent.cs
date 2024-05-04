namespace HexaEngine.Graphics
{
    using HexaEngine.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;

    public delegate void QueueIndexChangedEventHandler(IRendererComponent sender, uint oldIndex, uint newIndex);

    public interface IRendererComponent : IComponent, IHasFlags<RendererFlags>
    {
        public uint QueueIndex { get; }

        public bool BatchSupport { get; }

        public event QueueIndexChangedEventHandler? QueueIndexChanged;

        public BoundingBox BoundingBox { get; }

        public void Load(IGraphicsDevice device);

        public void Unload();

        public void Update(IGraphicsContext context);

        public void VisibilityTest(CullingContext context);

        public void DrawDepth(IGraphicsContext context);

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        public void Draw(IGraphicsContext context, RenderPath path);

        public void Bake(IGraphicsContext context);
        void Draw(IGraphicsContext deferred, string pass);

        public string DebugName { get; }
    }
}